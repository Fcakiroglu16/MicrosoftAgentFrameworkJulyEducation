using App.Console.Workflow.Executors;
using Microsoft.Agents.AI.Workflows;

namespace App.Console.Workflow.EdgeTypes;


public static class FanInSample
{
    private const string Input = "hello from the fan-in edge sample";

    private static Microsoft.Agents.AI.Workflows.Workflow BuildWorkflow()
    {
        var upperCaseExecutor = new UpperCaseExecutor();
        var truncateExecutor = new TruncateExecutor(maxLength: 15);
        var exclamationExecutor = new ExclamationExecutor();
        var fallbackExecutor = new FallbackExecutor();
        var aggregatorExecutor = new AggregatorExecutor();

        var builder = new WorkflowBuilder(upperCaseExecutor);
        builder
            // fan-out: send the same message to all three sources in parallel
            .AddFanOutEdge(upperCaseExecutor, targets: [truncateExecutor, exclamationExecutor, fallbackExecutor])
            // fan-in: each source's result is forwarded to the aggregator
            .AddFanInBarrierEdge([truncateExecutor, exclamationExecutor, fallbackExecutor], aggregatorExecutor);

        return builder
            .WithOutputFrom(aggregatorExecutor)
            .Build();
    }

    public static async Task Run()
    {
        var workflow = BuildWorkflow();

        var run = await InProcessExecution.RunAsync(workflow, Input);
        
        var outputEvents = run.NewEvents.OfType<WorkflowOutputEvent>().ToList();
        if (outputEvents.Count > 0)
        {
            System.Console.WriteLine("--- Fan-in Edge Output (final aggregate) ---");
            System.Console.WriteLine(outputEvents[^1].As<string>());
        }
    }
}
