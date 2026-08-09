using A2A;
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


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment()) app.MapOpenApi();

app.MapDefaultEndpoints();

// App2.API'deki agent'ı A2A protokolü ile keşfedip çağırır.
// Doküman: https://learn.microsoft.com/en-us/agent-framework/agents/providers/agent-to-agent
app.MapPost("/remote-agent/weather", async (
    AgentRequest request,
    IHttpClientFactory httpClientFactory,
    CancellationToken cancellationToken) =>
{
    var httpClient = httpClientFactory.CreateClient(RemoteAgent.HttpClientName);

    // Agent card'ı çözümleyip tek adımda bir AIAgent oluştur.
    var resolver = new A2ACardResolver(httpClient.BaseAddress!, httpClient, RemoteAgent.AgentCardPath);
    var agent = await resolver.GetAIAgentAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

    var result = await agent.RunAsync(request.Prompt, cancellationToken: cancellationToken).ConfigureAwait(false);

    return Results.Ok(new AgentResponse(result.Text));
});


app.Run();

internal static class RemoteAgent
{
    internal const string HttpClientName = "app2-api";
    internal const string AgentCardPath = "/a2a/weather/card";
}

internal sealed record AgentRequest(string Prompt);

internal sealed record AgentResponse(string Text);

internal sealed record SequentialOrderWorkflowRequest(string OrderRequest);