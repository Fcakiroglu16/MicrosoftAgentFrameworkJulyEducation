using App.API.Workflow;
using Microsoft.Extensions.AI;
using OpenAI;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();


var apiKey = Environment.GetEnvironmentVariable("OPEN_AI_KEY");
if (string.IsNullOrWhiteSpace(apiKey))
    throw new InvalidOperationException("Lütfen OPEN_AI_KEY ortam değişkenini ayarlayın.");

builder.Services.AddSingleton<IChatClient>(_ =>
    new OpenAIClient(apiKey)
        .GetChatClient("gpt-4o")
        .AsIChatClient());



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) app.MapOpenApi();

app.MapPost("/api/sequential-order-workflow", async (SequentialOrderWorkflowRequest request, IChatClient chatClient) =>
{
    if (string.IsNullOrWhiteSpace(request.OrderRequest))
        return Results.BadRequest("OrderRequest boş olamaz.");

    List<ChatMessage> result = await SequentialOrderWorkflow.ExecuteAsync(chatClient, request.OrderRequest);

    var response = result.Select(m => new AgentResponse(m.Text ?? string.Empty)).ToList();
    return Results.Ok(response);
})
.WithName("SequentialOrderWorkflow")
.WithSummary("Sipariş -> Stok Kontrolü -> Fatura sequential agent workflow'unu çalıştırır.");


app.Run();

internal sealed record AgentRequest(string Prompt);

internal sealed record AgentResponse(string Text);

internal sealed record SequentialOrderWorkflowRequest(string OrderRequest);