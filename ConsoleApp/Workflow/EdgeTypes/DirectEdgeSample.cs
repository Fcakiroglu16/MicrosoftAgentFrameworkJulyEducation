using App.Console.Workflow.Executors;
using Microsoft.Agents.AI.Workflows;

namespace App.Console.Workflow.EdgeTypes;


public static class DirectEdgeSample
{
    private const string Input = "hello from the direct edge sample";

    private static Microsoft.Agents.AI.Workflows.Workflow BuildWorkflow()
    {
        var upperCaseExecutor = new UpperCaseExecutor();
        var truncateExecutor = new TruncateExecutor(maxLength: 15);
        var exclamationExecutor = new ExclamationExecutor();

        return new WorkflowBuilder(upperCaseExecutor)
            .AddEdge(upperCaseExecutor, truncateExecutor)      // direct edge: upperCase -> truncate
            .AddEdge(truncateExecutor, exclamationExecutor)    // direct edge: truncate -> exclamation
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
                System.Console.WriteLine("--- Direct Edge Output ---");
                System.Console.WriteLine(outputEvent.As<string>());
            }
        }
    }
}
