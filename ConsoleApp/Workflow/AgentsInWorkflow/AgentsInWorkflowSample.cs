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
        Microsoft.Agents.AI.Workflows.Futures.EnableAgentResponseOutputTaggingAndFiltering = true;

        AIAgent translationAgent = AgentSetup.GetTranslationAgent("English");
        AIAgent upperCaseAgent = AgentSetup.GetUpperCaseAgent();
        AIAgent sentimentAgent = AgentSetup.GetStructuredSentimentAgent();

        var workflow = new WorkflowBuilder(translationAgent)
            .AddEdge(translationAgent, upperCaseAgent)
            .AddEdge(upperCaseAgent, sentimentAgent)
            .WithOutputFrom(sentimentAgent)
            .Build();
        
        
        
        // Execute the workflow (streaming)
        await using StreamingRun run = await InProcessExecution.RunStreamingAsync(workflow, new ChatMessage(ChatRole.User, Input));
        
        // Must send the turn token to trigger the agents.
        // The agents are wrapped as executors. When they receive messages,
        // they will cache the messages and only start processing when they receive a TurnToken.
        await run.TrySendMessageAsync(new TurnToken(emitEvents: true));
        await foreach (WorkflowEvent evt2 in run.WatchStreamAsync())
        {
            if (evt2 is AgentResponseUpdateEvent executorComplete)
            {
                System.Console.WriteLine($"{executorComplete.ExecutorId}: {executorComplete.Data}");
            }
        }
    }
}
