using Microsoft.Agents.AI.Workflows;

namespace App.API.Workflow;


public sealed class ExclamationExecutor() : Executor<string, string>("ExclamationExecutor")
{
    public override ValueTask<string> HandleAsync(
        string message,
        IWorkflowContext context,
        CancellationToken cancellationToken = default) =>
        new(message + "!");
}
