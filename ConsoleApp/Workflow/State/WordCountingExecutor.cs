using Microsoft.Agents.AI.Workflows;

namespace App.Console.Workflow.State;

/// <summary>
/// Receives a file id from <see cref="FileReadExecutor"/> and looks the
/// matching content back up from the shared "FileContent" state scope, then
/// returns the word count.
/// </summary>
public sealed class WordCountingExecutor() : Executor<string, int>("WordCountingExecutor")
{
    public override async ValueTask<int> HandleAsync(
        string message,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        // Retrieve the file content from the shared state using the same
        // scope name that FileReadExecutor wrote to.
        var fileContent = await context.ReadStateAsync<string>(message, scopeName: FileReadExecutor.FileContentScope, cancellationToken)
            ?? throw new InvalidOperationException("File content state not found");

        return fileContent.Split([' ', '\n', '\r'], StringSplitOptions.RemoveEmptyEntries).Length;
    }
}
