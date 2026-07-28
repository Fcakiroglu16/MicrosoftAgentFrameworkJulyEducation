using App.API.InMemoryChatHistory;
using App.API.ManagedStorageChatHistory;
using App.API.PersistenceChatHistory;
using Microsoft.Extensions.AI;
using OpenAI;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();
builder.Services.AddMemoryCache();
builder.Services.AddOpenApi();


// OpenAI
var openAiKey = Environment.GetEnvironmentVariable("OPEN_AI_KEY")
                ?? throw new InvalidOperationException("OPEN_AI_KEY environment variable is not set.");
var openAiClient = new OpenAIClient(openAiKey);
builder.Services.AddChatClient(openAiClient.GetChatClient("gpt-4o-mini").AsIChatClient());

builder.Services.AddInMemoryChatHistoryAgent();
builder.Services.AddPersistenceChatHistoryAgent();
builder.Services.AddManagedStorageChatHistoryAgent(openAiClient);



var app = builder.Build();



// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) app.MapOpenApi();

app.MapInMemoryChatHistoryEndpoints();
app.MapPersistenceChatHistoryEndpoints();
app.MapManagedStorageChatHistoryEndpoints();

app.Run();