using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using OpenAI;

namespace App.Console.McpToolCalling.Agents;


public static class McpPromptAgent
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


        await Prompts.ListPromptsAsync(client);

        AIAgent agent = new OpenAIClient(apiKey)
            .GetChatClient("gpt-4o-mini")
            .AsIChatClient()
            .AsAIAgent(
                instructions:
                "You are a general-purpose assistant with access to a set of tools. ");

    
        var brainstormSession = await agent.CreateSessionAsync();
        var brainstormMessages = await Prompts.GetPromptAsync(client, "brainstorm");
        var brainstormMessagesList = new List<ChatMessage>(brainstormMessages)
        {
            new(ChatRole.User, "Sürdürülebilir ve eğlenceli bir okul projesi için fikir üretelim.")
        };
        var brainstormResponse = await agent.RunAsync(brainstormMessagesList, brainstormSession);
        System.Console.WriteLine($"Agent (brainstorm): {brainstormResponse}");

    
        var explainSession = await agent.CreateSessionAsync();
        var explainMessages = await Prompts.GetPromptAsync(client, "explain_simply");
        var explainMessagesList = new List<ChatMessage>(explainMessages)
        {
            new(ChatRole.User, "Kuantum dolanıklığı nedir?")
        };
        var explainResponse = await agent.RunAsync(explainMessagesList, explainSession);
        System.Console.WriteLine($"Agent (explain_simply): {explainResponse}");

   
        var codeReviewSession = await agent.CreateSessionAsync();
        var codeReviewMessages = await Prompts.GetPromptAsync(client, "code_review", new Dictionary<string, object?>
        {
            ["language"] = "C#",
            ["code"] = "public int Divide(int a, int b) { return a / b; }"
        });
        var codeReviewResponse = await agent.RunAsync(codeReviewMessages, codeReviewSession);
        System.Console.WriteLine($"Agent (code_review): {codeReviewResponse}");
    }


}
