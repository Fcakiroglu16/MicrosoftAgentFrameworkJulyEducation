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

     
  
        
        
        
        
       
        AIAgent agent = new OpenAIClient(apiKey)
            .GetChatClient("gpt-4o-mini")
            .AsIChatClient()
            .AsAIAgent(
                instructions:
                "You are a general-purpose assistant with access to a set of tools. " +
                "For every user question, first check the available tools to see if one of them " +
                "can help answer it, and if so, call that tool before responding. " +
                "If none of the available tools are suitable for the question, do not answer it — " +
                "instead tell the user that you have no suitable tool to answer that question. " +
                "Never guess or make up information; only answer using what the tools return.");
        
        AgentSession session = await agent.CreateSessionAsync();

    }


}
