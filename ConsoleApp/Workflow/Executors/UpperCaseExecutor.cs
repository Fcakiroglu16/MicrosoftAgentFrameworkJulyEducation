using Microsoft.Agents.AI.Workflows;

namespace App.Console.Workflow.Executors;


public sealed class UpperCaseExecutor() : Executor<string, string>("UpperCaseExecutor")
{
    public override ValueTask<string> HandleAsync(string message, IWorkflowContext context, CancellationToken
        cancellationToken = default)
    {

        return new ValueTask<string>(message.ToUpperInvariant());
    }
}
      
