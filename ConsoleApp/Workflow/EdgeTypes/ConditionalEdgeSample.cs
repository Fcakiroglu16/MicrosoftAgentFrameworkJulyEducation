using Microsoft.Agents.AI.Workflows;

namespace App.Console.Workflow.EdgeTypes;


public static class ConditionalEdgeSample
{
    private const string Input = "hello from the conditional edge sample";

    private static Microsoft.Agents.AI.Workflows.Workflow BuildWorkflow()
    {
        var upperCaseExecutor = new UpperCaseExecutor();
        var truncateExecutor = new TruncateExecutor(maxLength: 15);
        var exclamationExecutor = new ExclamationExecutor();
        var fallbackExecutor = new FallbackExecutor();

        return new WorkflowBuilder(upperCaseExecutor)
            .AddEdge(upperCaseExecutor, truncateExecutor)
            // if: forward to exclamationExecutor when the truncated message is longer than 5 characters
            .AddEdge(truncateExecutor, exclamationExecutor, (string? message) => message?.Length > 5)
            // else: forward to fallbackExecutor when the condition above is NOT met
            .AddEdge(truncateExecutor, fallbackExecutor, (string? message) => message?.Length <= 5)
            .WithOutputFrom(exclamationExecutor, fallbackExecutor)
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
                System.Console.WriteLine("--- Conditional Edge Output ---");
                System.Console.WriteLine(outputEvent.As<string>());
            }
        }
    }
}
