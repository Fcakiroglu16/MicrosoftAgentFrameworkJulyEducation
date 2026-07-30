using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using OpenAI;

namespace App.Console.McpToolCalling;


public static class MicrosoftLearnMcp
{
    public static async Task RunAsync(string apiKey)
    {
        System.Console.WriteLine("=== Demo 1: Microsoft Learn Hosted MCP ===");
        System.Console.WriteLine();
        
        System.Console.WriteLine("Connecting to Microsoft Learn MCP server...");

        await using var mcpClient = await McpClient.CreateAsync(
            new HttpClientTransport(new HttpClientTransportOptions
            {
                Endpoint = new Uri("https://learn.microsoft.com/api/mcp"),
                Name     = "microsoft_learn"
            }));
        
        var allTools = await mcpClient.ListToolsAsync();

        System.Console.WriteLine($"Tools on server : {string.Join(", ", allTools.Select(t => t.Name))}");

        var docSearchTools = allTools
            .Where(t => t.Name == "microsoft_docs_search")
            .Cast<AITool>()
            .ToList();

        System.Console.WriteLine($"Tool in use     : microsoft_docs_search");
        System.Console.WriteLine();

        AIAgent agent = new OpenAIClient(apiKey)
            .GetChatClient("gpt-4o-mini")
            .AsIChatClient()
            .AsAIAgent(
                instructions:
                    "You answer questions by searching the Microsoft Learn documentation. " +
                    "Always use the microsoft_docs_search tool to retrieve accurate, up-to-date information " +
                    "before formulating your response. Cite the source URLs when available.",
                tools: docSearchTools);

        System.Console.WriteLine("Sending query to agent...");
        System.Console.WriteLine("Question: What is the Microsoft Agent Framework and how does it help build AI agents?");
        System.Console.WriteLine();

        AgentSession session = await agent.CreateSessionAsync();

        var response = await agent.RunAsync(
            "What is the Microsoft Agent Framework and how does it help build AI agents?",
            session);

        System.Console.WriteLine("Agent Response:");
        System.Console.WriteLine(response);
        System.Console.WriteLine();
    }
}
