using App.Console.Workflow.Executors;
using Microsoft.Agents.AI.Workflows;

namespace App.Console.Workflow.EdgeTypes;


public static class FanOutSample
{
    private const string Input = "hello from the fan-out edge sample";

    private static Func<string?, int, IEnumerable<int>> GetTargetSelector() =>
        (message, _) =>
        {
            // Both branches always run in parallel (index 0 and 1)
            List<int> targets = [0, 1];

            // Additionally fan out to the fallback branch (index 2) for short messages
            if (message?.Length < 10)
            {
                targets.Add(2);
            }

            return targets;
        };

    private static Microsoft.Agents.AI.Workflows.Workflow BuildWorkflow()
    {
        var upperCaseExecutor = new UpperCaseExecutor();
        var truncateExecutor = new TruncateExecutor(maxLength: 15);
        var exclamationExecutor = new ExclamationExecutor();
        var fallbackExecutor = new FallbackExecutor();

        var builder = new WorkflowBuilder(upperCaseExecutor);
        builder.AddFanOutEdge(
            upperCaseExecutor,
            targets: [truncateExecutor, exclamationExecutor, fallbackExecutor],
            targetSelector: GetTargetSelector());

        return builder
            .WithOutputFrom(truncateExecutor, exclamationExecutor, fallbackExecutor)
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
                System.Console.WriteLine("--- Fan-out Edge Output ---");
                System.Console.WriteLine(outputEvent.As<string>());
            }
        }
    }
}
