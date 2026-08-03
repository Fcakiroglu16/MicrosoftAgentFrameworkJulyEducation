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

        await foreach (WorkflowEvent evt in run.WatchStreamAsync())
        {
            if (evt is AgentResponseUpdateEvent executorComplete)
            {
                System.Console.Write(executorComplete.Update.Text);
            }
        }

        System.Console.WriteLine();
    }
}
