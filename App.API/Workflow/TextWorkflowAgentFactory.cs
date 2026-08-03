using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;

namespace App.API.Workflow;

/// <summary>
/// Builds the two-executor text workflow (UpperCase -> Truncate) and wraps
/// it as a <see cref="TextWorkflowAgent"/>.
/// </summary>
public static class TextWorkflowAgentFactory
{
    public static Microsoft.Agents.AI.Workflows.Workflow BuildWorkflow(int maxLength = 15)
    {
        var upperCaseExecutor = new UpperCaseExecutor();
        var truncateExecutor = new TruncateExecutor(maxLength);

        return new WorkflowBuilder(upperCaseExecutor)
            .AddEdge(upperCaseExecutor, truncateExecutor)
            .WithOutputFrom(truncateExecutor)
            .Build();
    }

    public static AIAgent CreateAgent(int maxLength = 15) => new TextWorkflowAgent(BuildWorkflow(maxLength));
}
