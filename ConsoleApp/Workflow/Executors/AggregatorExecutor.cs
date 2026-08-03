using Microsoft.Agents.AI.Workflows;

namespace App.Console.Workflow.Executors;

public sealed class AggregatorExecutor() : Executor<string, string>("AggregatorExecutor")
{
    private readonly List<string> _received = [];

    public override ValueTask<string> HandleAsync(
        string message,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        _received.Add(message);
        return new ValueTask<string>(string.Join(" + ", _received));
    }
}
