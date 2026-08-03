using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;

namespace App.Console.Workflow.AgentWorkflow.Executors;

/// <summary>
/// Workflow executor that delegates its work to an AI agent, classifying the
/// incoming sentence as Positive or Negative and returning a structured result.
/// </summary>
public sealed class SentimentAgentExecutor(AIAgent agent) : Executor<string, SentimentResult>("SentimentAgentExecutor")
{
    public override async ValueTask<SentimentResult> HandleAsync(
        string message,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        var response = await agent.RunAsync<SentimentResult>(message, cancellationToken: cancellationToken);
        return response.Result;
    }
}
