using Microsoft.Agents.AI.Workflows;

namespace App.API.Workflow;


public sealed class TruncateExecutor(int maxLength) : Executor<string, string>("TruncateExecutor")
{
    public override ValueTask<string> HandleAsync(
        string message,
        IWorkflowContext context,
        CancellationToken cancellationToken = default) =>
        new(message.Length <= maxLength ? message : message[..maxLength]);
}
