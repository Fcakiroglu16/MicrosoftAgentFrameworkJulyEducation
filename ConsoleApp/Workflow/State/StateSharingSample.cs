using Microsoft.Agents.AI.Workflows;

namespace App.Console.Workflow.State;

/// <summary>
/// Demonstrates workflow <b>State</b>: instead of passing large payloads
/// through direct messages, <see cref="FileReadExecutor"/> stores the file
/// content in a shared state scope ("FileContent") and only forwards a small
/// file id downstream. <see cref="WordCountingExecutor"/> then reads the same
/// scope, using that id, to get the content back and compute a word count.
/// </summary>
public static class StateSharingSample
{
    public static async Task Run()
    {
        var tempFilePath = Path.GetTempFileName();
        await File.WriteAllTextAsync(tempFilePath, "hello from the state sharing sample this is a simple demo");

        try
        {
            var fileReadExecutor = new FileReadExecutor();
            var wordCountingExecutor = new WordCountingExecutor();

            var workflow = new WorkflowBuilder(fileReadExecutor)
                .AddEdge(fileReadExecutor, wordCountingExecutor)   // only the file id travels through this edge
                .WithOutputFrom(wordCountingExecutor)
                .Build();

            var run = await InProcessExecution.RunAsync(workflow, tempFilePath);

            foreach (var workflowEvent in run.NewEvents)
            {
                if (workflowEvent is WorkflowOutputEvent outputEvent)
                {
                    System.Console.WriteLine("--- State Sharing Sample Output ---");
                    System.Console.WriteLine($"Word count: {outputEvent.As<int>()}");
                }
            }
        }
        finally
        {
            File.Delete(tempFilePath);
        }
    }
}
