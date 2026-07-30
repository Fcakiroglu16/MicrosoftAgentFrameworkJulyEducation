using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using OpenAI;

namespace App.Console.McpToolCalling.AgentsWithStreamableHttp;


public static class McpToolAgentWithStreamableHttp
{
    public static async Task RunAsync(string apiKey)
    {
       
        
        
        var transport = new HttpClientTransport(new HttpClientTransportOptions
        {
            Endpoint = new Uri("http://localhost:5000/mcp"),
            TransportMode = HttpTransportMode.StreamableHttp,
            ConnectionTimeout = TimeSpan.FromSeconds(30),
            AdditionalHeaders = new Dictionary<string, string>
            {
                ["X-Api-Key"] = "dev-secret-key-change-me"
            }
        });


        await using var client = await McpClient.CreateAsync(transport);
        
        await client.PingAsync();


        await Tools.ListToolAsync(client);

        var randomNumberToolResult = await client.CallToolAsync(
            "get_random_number",
            new Dictionary<string, object?> { ["min"] = 1, ["max"] = 50 });
        
        foreach (var content in randomNumberToolResult.Content.OfType<TextContentBlock>())
            System.Console.WriteLine($" get_random_number tool result  => {content.Text}");

        System.Console.WriteLine("#################################");
        
        
        var motivationalQuoteToolResult = await client.CallToolAsync(
            "get_motivational_quote",
            new Dictionary<string, object?>());
        
        foreach (var content in motivationalQuoteToolResult.Content.OfType<TextContentBlock>())
            System.Console.WriteLine($" get_motivational_quote tool result  => {content.Text}");
  
        
        
        
        
        var tools = await client.ListToolsAsync();
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
                "Never guess or make up information; only answer using what the tools return.",
                tools: tools.Cast<AITool>().ToList());
        
        AgentSession session = await agent.CreateSessionAsync();

        var randomNumberResponse = await agent.RunAsync(
            "1 ile 50 arasında rastgele bir sayı üret.",
            session);
        System.Console.WriteLine($"Agent (random number): {randomNumberResponse}");
        PrintToolUsage(randomNumberResponse);

        var motivationalQuoteResponse = await agent.RunAsync(
            "Bana motive edici bir söz söyler misin?",
            session);
        System.Console.WriteLine($"Agent (motivational quote): {motivationalQuoteResponse}");
        PrintToolUsage(motivationalQuoteResponse);
        
        
        
        
        var productResponse = await agent.RunAsync(
            "ismi kalem 1 olan fiyatı 300 tl olan ürünü ekle",
            session);
        System.Console.WriteLine($"Agent (Product ): {productResponse}");
        PrintToolUsage(productResponse);
    }

    /// <summary>
    /// Verilen ajan cevabında bir tool (function) çağrısı yapılıp yapılmadığını kontrol eder
    /// ve hangi tool'ların hangi argümanlarla çağrıldığını, sonuçlarıyla birlikte yazdırır.
    /// </summary>
    private static void PrintToolUsage(AgentResponse response)
    {
        var calls = response.Messages
            .SelectMany(m => m.Contents)
            .OfType<FunctionCallContent>()
            .ToList();

        var results = response.Messages
            .SelectMany(m => m.Contents)
            .OfType<FunctionResultContent>()
            .ToList();

        if (calls.Count == 0)
        {
            System.Console.WriteLine("  ⚠ Tool kullanılmadı — cevap doğrudan modelden geldi.");
            return;
        }

        foreach (var call in calls)
        {
            var args = string.Join(", ", call.Arguments?.Select(a => $"{a.Key}={a.Value}") ?? []);
            System.Console.WriteLine($"  ✔ Tool çağrıldı: {call.Name}({args})");
        }

        foreach (var result in results)
            System.Console.WriteLine($"  ↳ Tool sonucu (callId={result.CallId}): {result.Result}");
    }
}
