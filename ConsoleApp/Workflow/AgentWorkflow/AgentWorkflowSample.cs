using App.Console.Agents;
using App.Console.Workflow.AgentWorkflow.Executors;
using App.Console.Workflow.Executors;
using Microsoft.Agents.AI.Workflows;

namespace App.Console.Workflow.AgentWorkflow;


public static class AgentWorkflowSample
{
    private const string Input = "Bu ürünü gerçekten çok beğendim, harika bir deneyimdi!";

    private static Microsoft.Agents.AI.Workflows.Workflow BuildWorkflow()
    {
        var translationExecutor = new TranslationAgentExecutor(AgentSetup.GetTranslationAgent());
        var upperCaseExecutor = new UpperCaseExecutor();
        var sentimentExecutor = new SentimentAgentExecutor(AgentSetup.GetSentimentAgent());

        return new WorkflowBuilder(translationExecutor)
            .AddEdge(translationExecutor, upperCaseExecutor)
            .AddEdge(upperCaseExecutor, sentimentExecutor)
            .WithOutputFrom(sentimentExecutor)
            .Build();
    }

    public static async Task Run()
    {
        var workflow = BuildWorkflow();

        var run = await InProcessExecution.RunAsync(workflow, Input);

        foreach (var workflowEvent in run.NewEvents)
        {
            if (workflowEvent is WorkflowOutputEvent outputEvent)
            {
                var result = outputEvent.As<SentimentResult>();
                System.Console.WriteLine("--- Workflow Output ---");
                System.Console.WriteLine($"Sentiment: {result?.Sentiment}");
                System.Console.WriteLine($"Reasoning: {result?.Reasoning}");
            }
        }
    }
}
