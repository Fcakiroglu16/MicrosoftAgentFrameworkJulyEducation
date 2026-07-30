using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using OpenAI;
namespace App.Console.McpToolCalling;


public static class McpResourceAgent
{
    public static async Task RunAsync(string apiKey)
    {
        var serverProjectPath = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "ModelContextProtocol.McpServerWithStdio"));

        System.Console.WriteLine($" Base Directory :{AppContext.BaseDirectory}");
        
        
        System.Console.WriteLine($"Connecting to MCP server at: {serverProjectPath}");

        var transport = new StdioClientTransport(new StdioClientTransportOptions
        {
            Name = "Local Stdio MCP Server",
            Command = "dotnet",
            Arguments = ["run", "--project", serverProjectPath],
        });

        await using var client = await McpClient.CreateAsync(transport);
        
        await client.PingAsync();


       await Resources.ListStaticResourcesAsync(client);
     
       await Resources.ListAndReadTemplateResourcesAsync(client);
       var profileText = await Resources.ReadTemplateResourceAsync(client, "users://profiles/1");
     
       
        AIAgent agent = new OpenAIClient(apiKey)
            .GetChatClient("gpt-4o-mini")
            .AsIChatClient()
            .AsAIAgent(
                instructions:
                "You are a general-purpose assistant with access to a set of tools. ");
        
        AgentSession session = await agent.CreateSessionAsync();

        var profileQuestionResponse = await agent.RunAsync(
            $"Here is a user profile JSON:\n\n{profileText}\n\n" +
            "What is this user's name and what role do they have?",
            session);
        System.Console.WriteLine($"Agent (profile question): {profileQuestionResponse}");

    }


}
