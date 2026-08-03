using Microsoft.Agents.AI.Workflows;

namespace App.Console.Workflow.Mutability;

public static class ImmutableWorkflowSample
{
    private static Microsoft.Agents.AI.Workflows.Workflow BuildWorkflow()
    {
        var counterExecutor = new CounterExecutor();

        return new WorkflowBuilder(counterExecutor)
            .WithOutputFrom(counterExecutor)
            .Build();
    }

    public static async Task Run()
    {
        await RunReusingSameWorkflowInstance_AntiPattern();
        await RunWithFreshWorkflowPerRequest_RecommendedPattern();
    }

    
    private static async Task RunReusingSameWorkflowInstance_AntiPattern()
    {
        System.Console.WriteLine("--- Reusing a single Workflow instance (anti-pattern) ---");

        var workflow = BuildWorkflow();

        for (var i = 1; i <= 3; i++)
        {
            try
            {
                var run = await InProcessExecution.RunAsync(workflow, $"request {i}");
                foreach (var workflowEvent in run.NewEvents)
                {
                    if (workflowEvent is WorkflowOutputEvent outputEvent)
                        System.Console.WriteLine(outputEvent.As<string>());
                }
            }
            catch (InvalidOperationException ex)
            {
                System.Console.WriteLine($"request {i} FAILED: {ex.Message}");
            }
        }
    }

 
    private static async Task RunWithFreshWorkflowPerRequest_RecommendedPattern()
    {
        System.Console.WriteLine("--- Building a new Workflow per request (recommended) ---");

        for (var i = 1; i <= 3; i++)
        {
            var workflow = BuildWorkflow();
            var run = await InProcessExecution.RunAsync(workflow, $"request {i}");
            foreach (var workflowEvent in run.NewEvents)
            {
                if (workflowEvent is WorkflowOutputEvent outputEvent)
                    System.Console.WriteLine(outputEvent.As<string>());
            }
        }
    }
}
