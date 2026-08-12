using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Foundry.Hosting;
using Microsoft.Extensions.AI;
using OpenAI;
using SimpleAgentDeploy.Stock;

var builder = AgentHost.CreateBuilder(args);

var apiKey = Environment.GetEnvironmentVariable("OPEN_AI_KEY");
if (string.IsNullOrWhiteSpace(apiKey))
    throw new InvalidOperationException("Lütfen OPEN_AI_KEY ortam değişkenini ayarlayın.");

var chatClient = new OpenAIClient(apiKey)
    .GetChatClient("gpt-4o")
    .AsIChatClient();

var stockService = new StockService();


var aiAgent = chatClient.AsAIAgent(new ChatClientAgentOptions
{
    Name = "StockCheckAgent",
    Description = "Checks stock availability for the requested product and quantity by calling a tool.",
    ChatOptions = new ChatOptions
    {
        Instructions = """
                       You are a stock control assistant.
                       - You will receive a natural language question asking about the stock availability of a product (e.g. "5 adet Kalem stokta var mı?").
                       - You MUST extract the product name and quantity from the question, then call the CheckStock tool with them to verify availability.
                       - If the quantity is not specified, assume a quantity of 1.
                       - Report the order (product and quantity) together with the stock check result returned by the tool.
                       """,
        Tools = [AIFunctionFactory.Create(stockService.CheckStock)]
    }
});


builder.Services.AddFoundryResponses(aiAgent);
builder.RegisterProtocol("responses", endpoints => endpoints.MapFoundryResponses());

var app = builder.Build();


app.Run();