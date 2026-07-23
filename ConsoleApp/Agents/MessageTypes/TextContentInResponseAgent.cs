using System.Text;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace App.Console.Agents;

/// <summary>
/// DEMO 2 — TextContent: The Most Common Content Type
///
/// TextContent carries textual data. It appears in both input
/// (user prompts) and output (agent responses).
/// The .Text property on responses aggregates all TextContent.
/// </summary>
public static class TextContentInResponseAgent
{
    public static async Task RunAsync(ChatClientAgent agent)
    {
        System.Console.WriteLine("╔═══════════════════════════════════════════════╗");
        System.Console.WriteLine("║  DEMO 2 — TextContent in Agent Responses       ║");
        System.Console.WriteLine("╚═══════════════════════════════════════════════╝\n");

        const string prompt = "Asp.net Core Framework nedir?";
        AgentResponse response = await agent.RunAsync(prompt);

        // 2a. The shortcut: response.Text aggregates ALL TextContent
        System.Console.WriteLine("── 2a. response.Text (aggregated TextContent) ──");
        System.Console.WriteLine(response.Text);

        // 2b. What response.Text does internally: iterate messages → TextContent
        System.Console.WriteLine("\n── 2b. Manual TextContent extraction ──");
        var manualText = new StringBuilder();
        foreach (var msg in response.Messages)
        {
            
            System.Console.WriteLine($"\n  ChatMessage → Role: {msg.Role}, Contents: {msg.Contents.Count}");

            foreach (var content in msg.Contents)
            {
                if (content is TextContent tc)
                {
                    manualText.Append(tc.Text);
                }
                else
                {
                    System.Console.WriteLine($"    ├─ {content.GetType().Name} (not text — skipped by .Text)");
                }
            }
        }
        System.Console.WriteLine($"\n  Manual == response.Text? {manualText.ToString() == response.Text}");
    }
}
