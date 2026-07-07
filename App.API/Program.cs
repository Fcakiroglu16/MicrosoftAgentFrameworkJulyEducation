using Microsoft.Extensions.AI;
using OpenAI;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var apiKey = Environment.GetEnvironmentVariable("OPEN_AI_KEY")
             ?? throw new InvalidOperationException("OPEN_AI_KEY ortam değişkenini ayarlayın.");

builder.Services.AddChatClient(
        new OpenAIClient(apiKey).GetChatClient("gpt-4o").AsIChatClient())
    .UseFunctionInvocation().UseLogging();


var app = builder.Build();


app.MapGet("api/chat", async (IChatClient chatClient, string message) =>
{
    var response = await chatClient.GetResponseAsync(message);
    return response;
});


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


app.Run();

