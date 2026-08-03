using System.Text;
using System.Text.Json;
using App.Console.Agents;
using App.Console.Workflow.AgentWorkflow;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace App.Console.Workflow.AgentsInWorkflow;


public static class AgentsInWorkflowSample
{
    private const string Input = "Bu ürünü gerçekten çok beğendim, harika bir deneyimdi!";
    

    public static async Task Run()
    {
        AIAgent translationAgent = AgentSetup.GetTranslationAgent("English");
        AIAgent upperCaseAgent = AgentSetup.GetUpperCaseAgent();
        AIAgent sentimentAgent = AgentSetup.GetStructuredSentimentAgent();

        var workflow = new WorkflowBuilder(translationAgent)
            .AddEdge(translationAgent, upperCaseAgent)
            .AddEdge(upperCaseAgent, sentimentAgent)
            .Build();

        
        await using StreamingRun run = await InProcessExecution.RunStreamingAsync(
            workflow, new ChatMessage(ChatRole.User, Input));

        await run.TrySendMessageAsync(new TurnToken(emitEvents: true));

        // Only the last agent's (sentimentAgent) streamed text is what we care about;
        // the translator's and upper-caser's intermediate output is just plumbing.
        var sentimentJson = new StringBuilder();
        await foreach (WorkflowEvent evt in run.WatchStreamAsync())
        {
            if (evt is AgentResponseUpdateEvent { } update &&
                update.ExecutorId.StartsWith(sentimentAgent.Name + "_", StringComparison.Ordinal))
            {
                sentimentJson.Append(update.Update.Text);
            }
        }

        var result = JsonSerializer.Deserialize<SentimentResult>(
            sentimentJson.ToString(),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        System.Console.WriteLine("--- Workflow Output ---");
        System.Console.WriteLine($"Sentiment: {result?.Sentiment}");
        System.Console.WriteLine($"Reasoning: {result?.Reasoning}");
    }
}
