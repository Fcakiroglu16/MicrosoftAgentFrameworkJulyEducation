using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace App.Console.Workflow.WorkflowAsAgent;

/// <summary>
/// A plain (non-agent) executor inserted between the writer and reviewer agents.
/// It doesn't call any AI model - it just logs the word count of the writer's
/// draft and forwards the conversation unchanged, demonstrating that regular
/// executors can be freely mixed with agent nodes in the same workflow.
/// </summary>
public sealed class WordCountLoggingExecutor()
    : Executor<List<ChatMessage>, List<ChatMessage>>("WordCountLoggingExecutor")
{
    public override ValueTask<List<ChatMessage>> HandleAsync(
        List<ChatMessage> message,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        var lastText = message.Count > 0 ? message[^1].Text : string.Empty;
        var wordCount = lastText.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        System.Console.WriteLine($"[WordCountLoggingExecutor] Draft word count: {wordCount}");

        return new ValueTask<List<ChatMessage>>(message);
    }
}
