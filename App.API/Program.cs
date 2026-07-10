using App.API.Data;
using App.API.Services;
using Microsoft.Data.SqlTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using OpenAI;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("SqlServer")));


var apiKey = Environment.GetEnvironmentVariable("OPEN_AI_KEY")
             ?? throw new InvalidOperationException("OPEN_AI_KEY ortam değişkenini ayarlayın.");

var openAiClient = new OpenAIClient(apiKey);
var openAiChatClient = openAiClient.GetChatClient("gpt-4o");


builder.Services.AddChatClient(
        openAiChatClient.AsIChatClient())
    .UseFunctionInvocation().UseLogging().UseOpenTelemetry(sourceName: "chat-client-source");

builder.Services.AddSingleton(
    openAiClient.GetEmbeddingClient("text-embedding-3-small").AsIEmbeddingGenerator());


builder.Services.AddKeyedChatClient(
        "fast",
        new OpenAIClient(apiKey).GetChatClient("gpt-4o-mini").AsIChatClient())
    .UseFunctionInvocation();

builder.Services.AddKeyedChatClient(
        "smart",
        new OpenAIClient(apiKey).GetChatClient("gpt-4o").AsIChatClient())
    .UseFunctionInvocation();


var app = builder.Build();

app.MapDefaultEndpoints();


//app.MapPost()


app.MapGet("/chat/smart", async (string message,
    [FromKeyedServices("smart")] IChatClient chatClient) =>
{
    ChatResponse response = await chatClient.GetResponseAsync(message);
    return Results.Ok(new { model = "smart", reply = response.Text });
});

app.MapGet("/chat/fast", async (string message,
    [FromKeyedServices("fast")] IChatClient chatClient) =>
{
    ChatResponse response = await chatClient.GetResponseAsync(message);
    return Results.Ok(new { model = "fast", reply = response.Text });
});


app.MapGet("api/chat", async (IChatClient chatClient, string message) =>
{
    var response = await chatClient.GetResponseAsync(message);
    return response;
});

app.MapGet("/api/product/search", async (string q, AppDbContext dbContext,
    IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator) =>
{
    var queryResult = await embeddingGenerator.GenerateAsync([q]);

    var queryVector = new SqlVector<float>(queryResult[0].Vector);

    var results = await dbContext.Products
        .Where(p => p.NameEmbedding != null)
        .OrderBy(p => EF.Functions.VectorDistance("cosine", p.NameEmbedding!.Value, queryVector))
        .Take(5)
        .Select(p => new
        {
            p.Id,
            p.Name,
        })
        .ToListAsync();

    return Results.Ok(results);
});


app.MapPost("/api/product/seed",
    async (AppDbContext dbContext, IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator) =>
    {
        if (await dbContext.Products.AnyAsync())
            return Results.Ok("Products already seeded.");

        var products = new List<Product>();

        foreach (var name in SeedProduct.SampleProducts)
        {
            var result = await embeddingGenerator.GenerateAsync([name]);
            products.Add(new Product
            {
                Name = name,
                NameEmbedding = new SqlVector<float>(result[0].Vector)
            });
        }

        dbContext.Products.AddRange(products);
        await dbContext.SaveChangesAsync();

        return Results.Ok($"{products.Count} products seeded.");
    });


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


app.Run();

