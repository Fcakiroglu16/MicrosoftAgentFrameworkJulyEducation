using Azure.AI.Projects;
using Azure.Identity;
using Microsoft.Agents.AI;

var projectEndpoint = new Uri("https://julyeducation.services.ai.azure.com/api/projects/proj-july-education");
const string agentName = "AdvancedAgent";


var agentEndpoint = new Uri($"{projectEndpoint}/agents/{agentName}/endpoint/protocols/openai");

var projectClient = new AIProjectClient(projectEndpoint, new AzureCliCredential());

AIAgent agent = projectClient.AsAIAgent(agentEndpoint);

var session = await agent.CreateSessionAsync();

Console.WriteLine($"'{agentName}' agent'ına bağlanıldı. Çıkmak için 'exit' yazın.");
Console.WriteLine();

while (true)
{
    Console.Write("Siz: ");
    var userInput = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(userInput) || userInput.Equals("exit", StringComparison.OrdinalIgnoreCase)) break;

    var response = await agent.RunAsync(userInput, session);
    Console.WriteLine($"Agent: {response.Text}");
    Console.WriteLine();
}