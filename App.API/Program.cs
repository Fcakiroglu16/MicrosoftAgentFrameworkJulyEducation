

using System.Runtime.CompilerServices;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;

var builder = Microsoft.AspNetCore.Builder.WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddOpenApi();



var apiKey = Environment.GetEnvironmentVariable("OPEN_AI_KEY");
if (string.IsNullOrWhiteSpace(apiKey))
{
    throw new InvalidOperationException("Lütfen OPEN_AI_KEY ortam değişkenini ayarlayın.");
}

builder.Services.AddKeyedSingleton<AIAgent>("agent1", (_, _) =>
{
    IChatClient chatClient = new OpenAIClient(apiKey).GetChatClient("gpt-4o").AsIChatClient();

    var agent = chatClient.AsAIAgent(new ChatClientAgentOptions
    {
        Name = "ProjeKoordinatoru",
        Description =
            "Karmaşık iş ve projeleri gerçek dünya koşullarına uygun, net ve uygulanabilir adımlara bölen bir yapay zeka asistanı.",
        ChatOptions = new ChatOptions
        {
            Instructions = """
                           Sen deneyimli bir proje koordinatörüsün.
                           Kullanıcının verdiği her işi gerçek dünya koşullarına uygun,
                           net, ölçülebilir ve sıralı adımlara böl.
                           Her adımda sorumlu rolü, tahmini süreyi,
                           ihtiyaç duyulan kaynakları ve olası riskleri belirt.
                           Teknik jargon kullanma; tüm paydaşların anlayabileceği
                           açık ve profesyonel bir dille yaz.
                           """
        }
    });
    return agent;
});
builder.Services.AddKeyedSingleton<AIAgent>("agent2", (_, _) =>
{
    IChatClient chatClient = new OpenAIClient(apiKey).GetChatClient("gpt-4o").AsIChatClient();

    var agent = chatClient.AsAIAgent(new ChatClientAgentOptions
    {
        Name = "ProjeKoordinatoru",
        Description =
            "Karmaşık iş ve projeleri gerçek dünya koşullarına uygun, net ve uygulanabilir adımlara bölen bir yapay zeka asistanı.",
        ChatOptions = new ChatOptions
        {
            Instructions = """
                           Sen deneyimli bir proje koordinatörüsün.
                           Kullanıcının verdiği her işi gerçek dünya koşullarına uygun,
                           net, ölçülebilir ve sıralı adımlara böl.
                           Her adımda sorumlu rolü, tahmini süreyi,
                           ihtiyaç duyulan kaynakları ve olası riskleri belirt.
                           Teknik jargon kullanma; tüm paydaşların anlayabileceği
                           açık ve profesyonel bir dille yaz.
                           """
        }
    });
    return agent;
});



builder.Services.AddSingleton((_) =>
{
    IChatClient chatClient = new OpenAIClient(apiKey).GetChatClient("gpt-4o").AsIChatClient();

    var agent = chatClient.AsAIAgent(new ChatClientAgentOptions
    {
        Name = "ProjeKoordinatoru",
        Description =
            "Karmaşık iş ve projeleri gerçek dünya koşullarına uygun, net ve uygulanabilir adımlara bölen bir yapay zeka asistanı.",
        ChatOptions = new ChatOptions
        {
            Instructions = """
                           Sen deneyimli bir proje koordinatörüsün.
                           Kullanıcının verdiği her işi gerçek dünya koşullarına uygun,
                           net, ölçülebilir ve sıralı adımlara böl.
                           Her adımda sorumlu rolü, tahmini süreyi,
                           ihtiyaç duyulan kaynakları ve olası riskleri belirt.
                           Teknik jargon kullanma; tüm paydaşların anlayabileceği
                           açık ve profesyonel bir dille yaz.
                           """
        }
    });
    return agent;
});









var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}


var agentEndpoints = app.MapGroup("/api/agent");

agentEndpoints.MapPost("/non-streaming", async (
    AgentRequest request,
    [FromKeyedServices("agent1")] AIAgent agent,    [FromKeyedServices("agent2")] AIAgent agent2,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.Prompt))
    {
        return Results.BadRequest(new { Message = "Prompt boş olamaz." });
    }

    var response = await agent.RunAsync(
        request.Prompt,
        cancellationToken: cancellationToken);

    return Results.Ok(new AgentResponse(response.Text));
})
.WithName("RunAgentNonStreaming");




agentEndpoints.MapPost("/streaming", IResult (
    AgentRequest request,
    ChatClientAgent agent,
    CancellationToken cancellationToken) =>
{
    if (string.IsNullOrWhiteSpace(request.Prompt))
    {
        return Results.BadRequest(new { Message = "Prompt boş olamaz." });
    }

    return TypedResults.ServerSentEvents(
        StreamAgentUpdatesAsync(agent, request.Prompt, cancellationToken),
        eventType: "agent-update");
})
.WithName("RunAgentStreaming");



app.Run();


static async IAsyncEnumerable<string> StreamAgentUpdatesAsync(
    ChatClientAgent agent,
    string prompt,
    [EnumeratorCancellation] CancellationToken cancellationToken)
{
    await foreach (var update in agent.RunStreamingAsync(
        prompt,
        cancellationToken: cancellationToken))
    {
        yield return update.ToString();
    }
}
internal sealed record AgentRequest(string Prompt);
internal sealed record AgentResponse(string Text);
