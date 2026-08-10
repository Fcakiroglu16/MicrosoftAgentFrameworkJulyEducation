using A2A;
using A2A.AspNetCore;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Hosting;
using Microsoft.Extensions.AI;
using OpenAI;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var apiKey = Environment.GetEnvironmentVariable("OPEN_AI_KEY");
if (string.IsNullOrWhiteSpace(apiKey))
    throw new InvalidOperationException("Lütfen OPEN_AI_KEY ortam değişkenini ayarlayın.");

builder.Services.AddKeyedSingleton<IChatClient>(ChatClients.Gpt4oMini, (_, _) =>
    new OpenAIClient(apiKey)
        .GetChatClient("gpt-4o-mini")
        .AsIChatClient());

var weatherAgent = builder.AddAIAgent(
    "weather",
    instructions: "Sen bir hava durumu uzmanısın. Kullanıcının sorduğu şehir için kısa ve net hava durumu yorumu yap.",
    description: "Şehirler için kısa hava durumu yorumu yapan agent.",
    chatClientServiceKey: ChatClients.Gpt4oMini,
    lifetime: ServiceLifetime.Singleton);

weatherAgent.AddA2AServer(options =>
    options.ServerOptions = new A2AServerOptions { AutoAppendHistory = true });

var app = builder.Build();

app.MapDefaultEndpoints();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

var weatherA2AServer = app.Services.GetRequiredKeyedService<A2AServer>("weather");


var weatherA2ABaseUrl = Environment.GetEnvironmentVariable("ASPNETCORE_URLS")!
    .Split(';', StringSplitOptions.RemoveEmptyEntries)
    .First(u => u.StartsWith("https://", StringComparison.OrdinalIgnoreCase));

app.MapHttpA2A(weatherA2AServer, new AgentCard
{
    Name = "Weather Agent",
    Description = "Sehirler icin kisa hava durumu yorumu yapan agent.",
    Version = "1.0",
    // A2AClientFactory, agent card'i cozerken bu listede eslesen bir
    // protokol (HTTP+JSON / JSONRPC) arar. Bos birakilirsa A2AException firlar.
    SupportedInterfaces =
    [
        new AgentInterface
        {
            Url = $"{weatherA2ABaseUrl}/a2a/weather",
            ProtocolBinding = "HTTP+JSON"
        }
    ]
}, "/a2a/weather");

app.MapPost("/agents/weather", async (
    WeatherAgentRequest request,
    [FromKeyedServices("weather")] AIAgent agent,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.Prompt))
        return Results.BadRequest("Prompt boş olamaz.");

    var result = await agent.RunAsync(request.Prompt, cancellationToken: cancellationToken).ConfigureAwait(false);

    return Results.Ok(new WeatherAgentResponse(result.Text));
});

app.Run();

internal sealed record WeatherAgentRequest(string Prompt);
internal sealed record WeatherAgentResponse(string Text);

internal static class ChatClients
{
    internal const string Gpt4o = "gpt-4o";
    internal const string Gpt4oMini = "gpt-4o-mini";
}
