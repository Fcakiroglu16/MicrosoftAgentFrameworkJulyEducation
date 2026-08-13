using Microsoft.Agents.AI.Foundry.Hosting;
using Microsoft.Extensions.AI;
using OpenAI;

var apiKey = Environment.GetEnvironmentVariable("OPEN_AI_KEY");
if (string.IsNullOrWhiteSpace(apiKey))
    throw new InvalidOperationException("Lütfen OPEN_AI_KEY ortam değişkenini ayarlayın.");

var chatClient = new OpenAIClient(apiKey)
    .GetChatClient("gpt-4o")
    .AsIChatClient();
var agent = chatClient.AsAIAgent(
    "You are a helpful AI assistant.",
    "my-agent");


// var projectEndpoint = new Uri("https://julyeducation.services.ai.azure.com/api/projects/proj-july-education");
// var deployment = "gpt-5-mini";

// AIAgent agent = new AIProjectClient(projectEndpoint, new DefaultAzureCredential())
//     .AsAIAgent(
//         model: deployment,
//         instructions: "You are a helpful AI assistant.",
//         name: "my-agent");

var builder = AgentHost.CreateBuilder(args);
builder.Services.AddFoundryResponses(agent);
builder.RegisterProtocol("responses", endpoints => endpoints.MapFoundryResponses());

var app = builder.Build();
app.Run();