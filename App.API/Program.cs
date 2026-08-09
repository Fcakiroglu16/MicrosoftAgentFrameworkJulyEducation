using A2A;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Agents.AI;
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


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) app.MapOpenApi();

app.MapDefaultEndpoints();

// App2.API'deki agent'ı A2A protokolü ile keşfedip çağırır.
// Doküman: https://learn.microsoft.com/en-us/agent-framework/agents/providers/agent-to-agent
app.MapPost("/remote-agent/weather", async (
    AgentRequest request,
    [FromKeyedServices(RemoteAgent.AgentKey)] AIAgent agent,
    CancellationToken cancellationToken) =>
{
    var result = await agent.RunAsync(request.Prompt, cancellationToken: cancellationToken).ConfigureAwait(false);

    return Results.Ok(new AgentResponse(result.Text));
});


app.Run();

internal static class RemoteAgent
{
    internal const string HttpClientName = "app2-api";
    internal const string AgentCardPath = "/a2a/weather/card";
    internal const string AgentKey = "weather-remote";
}

internal sealed record AgentRequest(string Prompt);

internal sealed record AgentResponse(string Text);

internal sealed record SequentialOrderWorkflowRequest(string OrderRequest);