using App.API.Data;
using App.API.Endpoints;
using App.API.Services;
using Microsoft.Data.SqlTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using OpenAI;
using WebApplication.API.Endpoints;

var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

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

;
builder.Services.AddEmbeddingGenerator(
    openAiClient.GetEmbeddingClient("text-embedding-3-small").AsIEmbeddingGenerator());

builder.Services.AddSingleton<PdfProcessingService>();
builder.Services.AddSingleton<EmbeddingService>();
builder.Services.AddScoped<RagService>();
builder.Services.AddScoped<VectorSearchService>();
var app = builder.Build();


app.MapDefaultEndpoints();
app.MapChatEndpoints();
app.MapDocumentEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


app.Run();

sealed record KeywordSearchResult(int Id, string Name, int Rank);