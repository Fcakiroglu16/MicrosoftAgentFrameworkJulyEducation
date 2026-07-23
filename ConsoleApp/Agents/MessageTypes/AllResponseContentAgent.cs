using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace App.Console.Agents;

public static class AllResponseContentAgent
{
    public static async Task RunAsync(ChatClientAgent agent)
    {
        System.Console.WriteLine("╔═══════════════════════════════════════════════╗");
        System.Console.WriteLine("║  DEMO 3 — Inspecting All Content Types          ║");
        System.Console.WriteLine("╚═══════════════════════════════════════════════╝\n");

        const string prompt = "Asp.net Core Framework nedir?";
        AgentResponse response = await agent.RunAsync(prompt);

        System.Console.WriteLine($"Messages in response: {response.Messages.Count}\n");

        int textCount = 0, dataCount = 0, uriCount = 0, funcCallCount = 0, funcResultCount = 0, usageCount = 0, otherCount = 0;

        foreach (var message in response.Messages)
        {
            System.Console.WriteLine($"┌─ ChatMessage [Role: {message.Role}]");
            System.Console.WriteLine($"│  Content items: {message.Contents.Count}");

            foreach (var content in message.Contents)
            {
                switch (content)
                {
                    case TextContent tc:
                        textCount++;
                        System.Console.WriteLine($"│  ├─ TextContent");
                        System.Console.WriteLine($"│  │    Text: \"{AgentSetup.Truncate(tc.Text)}\"");
                        break;

                    case DataContent dc:
                        dataCount++;
                        System.Console.WriteLine($"│  ├─ DataContent");
                        System.Console.WriteLine($"│  │    MediaType: {dc.MediaType}");
                        System.Console.WriteLine($"│  │    HasData: {dc.Data.Length > 0}");
                        break;

                    case UriContent urc:
                        uriCount++;
                        System.Console.WriteLine($"│  ├─ UriContent");
                        System.Console.WriteLine($"│  │    Uri: {urc.Uri}");
               
                        break;

                    case FunctionCallContent fc:
                        funcCallCount++;
                        System.Console.WriteLine($"│  ├─ FunctionCallContent");
                        System.Console.WriteLine($"│  │    Name: {fc.Name}");
                        System.Console.WriteLine($"│  │    CallId: {fc.CallId}");
                        System.Console.WriteLine($"│  │    Arguments: {fc.Arguments}");
                        break;

                    case FunctionResultContent fr:
                        funcResultCount++;
                        System.Console.WriteLine($"│  ├─ FunctionResultContent");
                        System.Console.WriteLine($"│  │    CallId: {fr.CallId}");
                        System.Console.WriteLine($"│  │    Result: {fr.Result}");
                        break;

                    case UsageContent us:
                        usageCount++;
                        System.Console.WriteLine($"│  ├─ UsageContent");
                        System.Console.WriteLine($"│  │    Input: {us.Details.InputTokenCount}, Output: {us.Details.OutputTokenCount}");
                        break;

                    default:
                        otherCount++;
                        System.Console.WriteLine($"│  ├─ {content.GetType().Name} (unknown/custom)");
                        break;
                }
            }

            System.Console.WriteLine("└──────────────────────────────────");
        }

        System.Console.WriteLine("\n── Content Type Summary ──");
        System.Console.WriteLine($"  TextContent           : {textCount}");
        System.Console.WriteLine($"  DataContent           : {dataCount}");
        System.Console.WriteLine($"  UriContent            : {uriCount}");
        System.Console.WriteLine($"  FunctionCallContent   : {funcCallCount}");
        System.Console.WriteLine($"  FunctionResultContent : {funcResultCount}");
        System.Console.WriteLine($"  UsageContent          : {usageCount}");
        System.Console.WriteLine($"  Other                 : {otherCount}");
    }
}
