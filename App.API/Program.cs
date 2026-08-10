using A2A;
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

builder.Services.AddSingleton<IChatClient>(_ =>
    new OpenAIClient(apiKey)
        .GetChatClient("gpt-4o")
        .AsIChatClient());


builder.Services.AddHttpClient(RemoteAgent.HttpClientName,
    client => client.BaseAddress = new Uri("https+http://app2-api"));

// Uzak agent'ı keyed singleton olarak DI container'a kaydet.
builder.Services.AddKeyedSingleton<AIAgent>(RemoteAgent.AgentKey, (sp, _) =>
{
    var httpClient = sp.GetRequiredService<IHttpClientFactory>().CreateClient(RemoteAgent.HttpClientName);
    var resolver = new A2ACardResolver(httpClient.BaseAddress!, httpClient, RemoteAgent.AgentCardPath);
    return resolver.GetAIAgentAsync().GetAwaiter().GetResult();
});


builder.Services.AddKeyedSingleton<AIAgent>(TranslationAgent.AgentKey, (sp, _) =>
{
    var chatClient = sp.GetRequiredService<IChatClient>();
    return chatClient.AsAIAgent(new ChatClientAgentOptions
    {
        Name = "WeatherTranslatorAgent",
        Description = "Bir önceki agent'ın hava durumu cevabını Fransızcaya çevirir.",
        ChatOptions = new ChatOptions
        {
            Instructions = """
                           You are a translation assistant.
                           - You will receive the previous agent's weather response.
                           - Translate that response into French.
                           - Only return the translated French text, without any additional commentary.
                           """
        }
    });
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) app.MapOpenApi();

app.MapDefaultEndpoints();

app.MapPost("/remote-agent/weather", async (
    AgentRequest request,
    [FromKeyedServices(RemoteAgent.AgentKey)] AIAgent agent,
    CancellationToken cancellationToken) =>
{
    var result = await agent.RunAsync(request.Prompt, cancellationToken: cancellationToken);

    return Results.Ok(new AgentResponse(result.Text));
});


app.MapPost("/orchestration/weather-french", async (
    AgentRequest request,
    [FromKeyedServices(RemoteAgent.AgentKey)] AIAgent weatherAgent,
    [FromKeyedServices(TranslationAgent.AgentKey)] AIAgent translatorAgent,
    CancellationToken cancellationToken) =>
{
    var workflow = AgentWorkflowBuilder.BuildSequential([weatherAgent, translatorAgent]);

    var messages = new List<ChatMessage> { new(ChatRole.User, request.Prompt) };

    await using StreamingRun run = await InProcessExecution.RunStreamingAsync(workflow, messages, cancellationToken: cancellationToken);
    await run.TrySendMessageAsync(new TurnToken(emitEvents: true));

    List<ChatMessage>? result = null;
    await foreach (WorkflowEvent evt in run.WatchStreamAsync(cancellationToken))
    {
        if (evt is WorkflowOutputEvent outputEvt)
        {
            result = outputEvt.As<List<ChatMessage>>();
            break;
        }
    }

    var finalText = result?.LastOrDefault()?.Text ?? string.Empty;

    return Results.Ok(new AgentResponse(finalText));
});


app.Run();

internal static class RemoteAgent
{
    internal const string HttpClientName = "app2-api";
    internal const string AgentCardPath = "/a2a/weather/card";
    internal const string AgentKey = "weather-remote";
}

internal static class TranslationAgent
{
    internal const string AgentKey = "weather-translator-fr";
}

internal sealed record AgentRequest(string Prompt);

internal sealed record AgentResponse(string Text);

internal sealed record SequentialOrderWorkflowRequest(string OrderRequest);