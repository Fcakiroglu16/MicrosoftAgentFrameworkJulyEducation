using Microsoft.Agents.AI.Workflows;

namespace App.Console.Workflow.Executors;


public sealed class FallbackExecutor() : Executor<string, string>("FallbackExecutor")
{
    public override ValueTask<string> HandleAsync(string message, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        return new ValueTask<string>($"TOO SHORT: {message}");
    }
}
