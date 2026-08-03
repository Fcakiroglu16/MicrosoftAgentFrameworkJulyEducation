using Microsoft.Agents.AI.Workflows;

namespace App.Console.Workflow;


public sealed class ExclamationExecutor() : Executor<string, string>("ExclamationExecutor")
{
    public override ValueTask<string> HandleAsync(string message, IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        return new ValueTask<string>(message + "!");
    }
}
