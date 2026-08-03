using Microsoft.Agents.AI.Workflows;

namespace App.Console.Workflow.Mutability;

/// <summary>
/// A deliberately stateful executor (keeps a mutable call counter). It is used
/// to make the difference between a "fresh workflow per run" and a
/// "reused workflow instance" visible: since executor instances are captured
/// by the workflow at build time, reusing the same built <see cref="Workflow"/>
/// means reusing the same executor instances too, so their internal state
/// leaks across runs.
/// </summary>
public sealed class CounterExecutor() : Executor<string, string>("CounterExecutor")
{
    private int _callCount;

    public override ValueTask<string> HandleAsync(
        string message,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        _callCount++;
        return new ValueTask<string>($"{message} (call #{_callCount})");
    }
}
