using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using OpenAI;

namespace App.Console.McpToolCalling;


public static class GitHubMcp
{
    public static async Task RunAsync(string apiKey)
    {
        System.Console.WriteLine("=== Demo 2: GitHub Hosted MCP — Multi-Turn Research ===");
        System.Console.WriteLine();

        var githubPat = Environment.GetEnvironmentVariable("GITHUB_PAT")
            ?? throw new InvalidOperationException(
                "Set the GITHUB_PAT environment variable. " +
                "Create a token at https://github.com/settings/tokens");


        System.Console.WriteLine("Connecting to GitHub MCP server...");

        await using var mcpClient = await McpClient.CreateAsync(
            new HttpClientTransport(new HttpClientTransportOptions
            {
                Endpoint          = new Uri("https://api.githubcopilot.com/mcp/"),
                Name              = "github",
                AdditionalHeaders = new Dictionary<string, string>
                {
                    ["Authorization"] = $"Bearer {githubPat}"
                }
            }));

        // 2. List available tools and pass all of them to the agent.
        var tools = await mcpClient.ListToolsAsync();
        System.Console.WriteLine($"GitHub MCP tools available ({tools.Count}): {string.Join(", ", tools.Select(t => t.Name))}");
        System.Console.WriteLine();

        // 3. Build the agent with OpenAI.
        //    All MCP tools are registered on the agent at construction time.
        AIAgent agent = new OpenAIClient(apiKey)
            .GetChatClient("gpt-4o-mini")
            .AsIChatClient()
            .AsAIAgent(
                instructions:
                    "You are a GitHub research assistant. " +
                    "Use the available GitHub tools to search repositories, read files, and explore code. " +
                    "Always look up information before answering — never guess repository details.",
                tools: tools.Cast<AITool>().ToList());
        
        AgentSession session = await agent.CreateSessionAsync();

        // Turn 1 — Discover repositories
        System.Console.WriteLine("Turn 1: Searching for Agent Framework repositories...");
        var response1 = await agent.RunAsync(
            "Search GitHub for public repositories about 'Microsoft Agent Framework'. " +
            "List the top 3 results with their descriptions and star counts.",
            session);

        System.Console.WriteLine("Agent:");
        System.Console.WriteLine(response1);
        System.Console.WriteLine();

        // Turn 2 — List the authenticated user's private repositories, sorted by name
        System.Console.WriteLine("Turn 2: Listing private repositories (sorted)...");
        var response2 = await agent.RunAsync(
            "List all private repositories owned by the authenticated GitHub user. " +
            "Sort them alphabetically by repository name. " +
            "Return ONLY the repository names, one per line, with no numbering, " +
            "no descriptions and no extra commentary.",
            session);

        var repoNames = response2.ToString()
            .Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();

        System.Console.WriteLine("Private repositories:");
        for (var i = 0; i < repoNames.Count; i++)
        {
            System.Console.WriteLine($"{i + 1}. {repoNames[i]}");
        }
        System.Console.WriteLine();
    }
}
