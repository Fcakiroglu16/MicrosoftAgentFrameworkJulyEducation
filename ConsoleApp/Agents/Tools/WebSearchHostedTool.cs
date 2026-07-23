using System.Diagnostics.CodeAnalysis;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;

namespace App.Console.Agents.Tools;

/// <summary>
/// Lesson 10 — Hosted Tools
/// Demo 1: Simple agent using the hosted Web Search tool so it can answer
/// questions about current events using live information from the web.
/// </summary>
public static class WebSearchHostedTool
{
    [Experimental("OPENAI001")]
    public static async Task RunAsync()
    {
        var apiKey = Environment.GetEnvironmentVariable("OPEN_AI_KEY")
                     ?? throw new InvalidOperationException(
                         "Lütfen OPEN_AI_KEY ortam değişkenini ayarlayın.");
        System.Console.WriteLine("=== Demo 1: Web Search ===");
        System.Console.WriteLine();

        // The hosted web search tool requires the OpenAI Responses API.
        AIAgent agent = new OpenAIClient(apiKey)
            .GetResponsesClient()
            .AsIChatClient("gpt-4o-mini")
            .AsAIAgent(
                instructions: "You are a helpful assistant that can search the web for current information.",
                tools: [new HostedWebSearchTool()]);

        System.Console.WriteLine("Question: What is the current weather in Seattle?");
        System.Console.WriteLine();

        var response = await agent.RunAsync("What is the current weather in Seattle?");

        System.Console.WriteLine("Agent Response:");
        System.Console.WriteLine(response);
        System.Console.WriteLine();
    }
}
