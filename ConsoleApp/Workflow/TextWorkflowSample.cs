using Microsoft.Agents.AI.Workflows;

namespace App.Console.Workflow;


public static class TextWorkflowSample
{
    private const string Input = "hello from the agent workflow sample";

    private static Microsoft.Agents.AI.Workflows.Workflow BuildWorkflow()
    {
        var upperCaseExecutor = new UpperCaseExecutor();
        var truncateExecutor = new TruncateExecutor(maxLength: 15);

        return new WorkflowBuilder(upperCaseExecutor)
            .AddEdge(upperCaseExecutor, truncateExecutor)
            .WithOutputFrom(truncateExecutor)
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

    /// <summary>
    /// Alternative way to consume the workflow: instead of running to completion
    /// and inspecting the events afterwards, this streams every WorkflowEvent
    /// as it happens using RunStreamingAsync + WatchStreamAsync.
    /// </summary>
    public static async Task RunStreaming()
    {
        var workflow = BuildWorkflow();

        await using var run = await InProcessExecution.RunStreamingAsync(workflow, Input);

        await foreach (var workflowEvent in run.WatchStreamAsync())
        {
            System.Console.WriteLine($"[Event] {workflowEvent.GetType().Name}: {workflowEvent}");

            if (workflowEvent is WorkflowOutputEvent outputEvent)
            {
                System.Console.WriteLine("--- Workflow Output ---");
                System.Console.WriteLine(outputEvent.As<string>());
            }
        }
    }
}
