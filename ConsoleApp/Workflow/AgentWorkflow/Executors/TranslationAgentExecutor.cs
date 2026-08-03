using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;

namespace App.Console.Workflow.AgentWorkflow.Executors;

/// <summary>
/// Workflow executor that delegates its work to an AI agent, translating the
/// incoming Turkish sentence into English.
/// </summary>
public sealed class TranslationAgentExecutor(AIAgent agent) : Executor<string, string>("TranslationAgentExecutor")
{
    public override async ValueTask<string> HandleAsync(
        string message,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        var response = await agent.RunAsync(message, cancellationToken: cancellationToken);
        return response.Text;
    }
}
