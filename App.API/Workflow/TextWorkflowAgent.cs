using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace App.API.Workflow;

/// <summary>
/// Adapts the two-executor text workflow (UpperCase -> Truncate) to the
/// <see cref="AIAgent"/> abstraction, so it can be hosted, injected and
/// invoked through minimal API endpoints exactly like any other agent
/// (e.g. an LLM-backed <c>ChatClientAgent</c>).
/// </summary>
public sealed class TextWorkflowAgent(Microsoft.Agents.AI.Workflows.Workflow workflow) : AIAgent
{
    protected override string? IdCore => "text-workflow-agent";

    public override string Name => "Text Workflow Agent";

    public override string? Description =>
        "Converts input text to upper case and truncates it via a two-step workflow.";

    protected override ValueTask<AgentSession> CreateSessionCoreAsync(CancellationToken cancellationToken = default) =>
        new(new TextWorkflowSession());

    protected override ValueTask<JsonElement> SerializeSessionCoreAsync(
        AgentSession session,
        JsonSerializerOptions? jsonSerializerOptions = null,
        CancellationToken cancellationToken = default) =>
        // The workflow is stateless, so there is nothing to persist between turns.
        new(JsonDocument.Parse("{}").RootElement);

    protected override ValueTask<AgentSession> DeserializeSessionCoreAsync(
        JsonElement serializedState,
        JsonSerializerOptions? jsonSerializerOptions = null,
        CancellationToken cancellationToken = default) =>
        new(new TextWorkflowSession());

    protected override async Task<Microsoft.Agents.AI.AgentResponse> RunCoreAsync(
        IEnumerable<ChatMessage> messages,
        AgentSession? session = null,
        AgentRunOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var text = messages.LastOrDefault(m => m.Role == ChatRole.User)?.Text ?? string.Empty;

        var run = await InProcessExecution.RunAsync(workflow, text, cancellationToken: cancellationToken);

        var resultText = run.NewEvents.OfType<WorkflowOutputEvent>().FirstOrDefault()?.As<string>() ?? string.Empty;

        return new Microsoft.Agents.AI.AgentResponse(new ChatMessage(ChatRole.Assistant, resultText));
    }

    protected override async IAsyncEnumerable<AgentResponseUpdate> RunCoreStreamingAsync(
        IEnumerable<ChatMessage> messages,
        AgentSession? session = null,
        AgentRunOptions? options = null,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var response = await RunCoreAsync(messages, session, options, cancellationToken);
        foreach (var update in response.ToAgentResponseUpdates())
            yield return update;
    }

    private sealed class TextWorkflowSession : AgentSession;
}
