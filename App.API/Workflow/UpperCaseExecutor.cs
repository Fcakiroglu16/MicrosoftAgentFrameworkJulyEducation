using Microsoft.Agents.AI.Workflows;

namespace App.API.Workflow;


public sealed class UpperCaseExecutor() : Executor<string, string>("UpperCaseExecutor")
{
    public override ValueTask<string> HandleAsync(
        string message,
        IWorkflowContext context,
        CancellationToken cancellationToken = default) =>
        new(message.ToUpperInvariant());
}
