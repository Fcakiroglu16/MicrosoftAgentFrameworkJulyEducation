using App.API.Workflow;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;
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


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) app.MapOpenApi();



var workflowEndpoints = app.MapGroup("/api/workflow");

workflowEndpoints.MapPost("/direct-edge", async (
        AgentRequest request,
        CancellationToken cancellationToken) =>
    {
        if (string.IsNullOrWhiteSpace(request.Prompt))
            return Results.BadRequest(new { Message = "Prompt boş olamaz." });

        var upperCaseExecutor = new UpperCaseExecutor();
        var truncateExecutor = new TruncateExecutor(maxLength: 15);
        var exclamationExecutor = new ExclamationExecutor();

        var workflow = new WorkflowBuilder(upperCaseExecutor)
            .AddEdge(upperCaseExecutor, truncateExecutor)
            .AddEdge(truncateExecutor, exclamationExecutor)
            .WithOutputFrom(exclamationExecutor)
            .Build();

        var run = await InProcessExecution.RunAsync(workflow, request.Prompt, cancellationToken: cancellationToken);

        var resultText = run.NewEvents.OfType<WorkflowOutputEvent>().FirstOrDefault()?.As<string>() ?? string.Empty;

        return Results.Ok(new AgentResponse(resultText));
    })
    .WithName("RunDirectEdgeWorkflow");


app.Run();

internal sealed record AgentRequest(string Prompt);

internal sealed record AgentResponse(string Text);