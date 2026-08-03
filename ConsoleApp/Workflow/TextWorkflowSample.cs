using Microsoft.Agents.AI.Workflows;

namespace App.Console.Workflow;


public static class TextWorkflowSample
{
    private const string Input = "hello from the agent workflow sample";

    private static Microsoft.Agents.AI.Workflows.Workflow BuildWorkflow()
    {
        var upperCaseExecutor = new UpperCaseExecutor();
        var truncateExecutor = new TruncateExecutor(maxLength: 15);
        var exclamationExecutor = new ExclamationExecutor();

        return new WorkflowBuilder(upperCaseExecutor)
            .AddEdge(upperCaseExecutor, truncateExecutor)
            .AddEdge(truncateExecutor, exclamationExecutor)
            .WithOutputFrom(exclamationExecutor)
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
                System.Console.WriteLine("--- Workflow Output ---");
                System.Console.WriteLine(outputEvent.As<string>());
            }
        }
    }
    
}
