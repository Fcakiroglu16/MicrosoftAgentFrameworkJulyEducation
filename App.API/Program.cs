using App.API.Workflow;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.DependencyInjection;
using OpenAI;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();


var apiKey = Environment.GetEnvironmentVariable("OPEN_AI_KEY");
if (string.IsNullOrWhiteSpace(apiKey))
    throw new InvalidOperationException("Lütfen OPEN_AI_KEY ortam değişkenini ayarlayın.");

builder.Services.AddSingleton<AIAgent>(_ =>
{
    var chatClient = new OpenAIClient(apiKey).GetChatClient("gpt-4o").AsIChatClient();

    var agent = chatClient.AsAIAgent(new ChatClientAgentOptions
    {
        Name = "Simple Agent",
        Description =
            "Simple Agent",
        ChatOptions = new ChatOptions
        {
            Instructions = """
                           Simple Agent
                           """
        }
    });
    return agent;
});


builder.Services.AddKeyedSingleton<AIAgent>("text-workflow", (_, _) => TextWorkflowAgentFactory.CreateAgent());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) app.MapOpenApi();


var agentEndpoints = app.MapGroup("/api/agent");




var workflowAgentEndpoints = app.MapGroup("/api/workflow-agent");

workflowAgentEndpoints.MapPost("/non-streaming", async (
        AgentRequest request,
        [FromKeyedServices("text-workflow")] AIAgent agent,
        CancellationToken cancellationToken) =>
    {
        if (string.IsNullOrWhiteSpace(request.Prompt))
            return Results.BadRequest(new { Message = "Prompt boş olamaz." });

        var response = await agent.RunAsync(
            request.Prompt,
            cancellationToken: cancellationToken);

        return Results.Ok(new AgentResponse(response.Text));
    })
    .WithName("RunWorkflowAgentNonStreaming");


app.Run();

internal sealed record AgentRequest(string Prompt);

internal sealed record AgentResponse(string Text);