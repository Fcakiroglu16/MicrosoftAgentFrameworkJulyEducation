using Microsoft.Agents.AI.Workflows;

namespace App.Console.Workflow.State;

/// <summary>
/// Reads a file's content and stores it in a shared state scope, keyed by a
/// freshly generated file id. Only the id (not the full content) is passed
/// downstream via the direct edge, keeping the message small.
/// </summary>
public sealed class FileReadExecutor() : Executor<string, string>("FileReadExecutor")
{
    public const string FileContentScope = "FileContent";

    public override async ValueTask<string> HandleAsync(
        string message,
        IWorkflowContext context,
        CancellationToken cancellationToken = default)
    {
        // Read file content from disk.
        var fileContent = await System.IO.File.ReadAllTextAsync(message, cancellationToken);

        // Store the file content in a shared state scope so any other
        // executor using the same scope name can read it back.
        var fileId = Guid.NewGuid().ToString("N");
        await context.QueueStateUpdateAsync(fileId, fileContent, scopeName: FileContentScope, cancellationToken);

        return fileId;
    }
}
