using App.Console.Workflow.Executors;
using Microsoft.Agents.AI.Workflows;

namespace App.Console.Workflow.EdgeTypes;


public static class SwitchCaseSample
{
    private const string Input = "hello from the switch case edge sample";

    private static Microsoft.Agents.AI.Workflows.Workflow BuildWorkflow()
    {
        var upperCaseExecutor = new UpperCaseExecutor();

        var shortCaseExecutor = new FallbackExecutor();                // case: short message
        var mediumCaseExecutor = new TruncateExecutor(maxLength: 15);  // case: medium message
        var longCaseExecutor = new ExclamationExecutor();              // default: long message

        var builder = new WorkflowBuilder(upperCaseExecutor);
        builder.AddSwitch(upperCaseExecutor, switchBuilder => switchBuilder
            .AddCase((string? message) => message?.Length < 10, [shortCaseExecutor])
            .AddCase((string? message) => message?.Length < 25, [mediumCaseExecutor])
            .WithDefault([longCaseExecutor]));

        return builder
            .WithOutputFrom(shortCaseExecutor, mediumCaseExecutor, longCaseExecutor)
            .Build();
    }

    public static async Task Run()
    {
        var workflow = BuildWorkflow();

        var run = await InProcessExecution.RunAsync(workflow, Input);

        foreach (var workflowEvent in run.NewEvents)
        {
            if (workflowEvent is WorkflowOutputEvent outputEvent)
            {
                System.Console.WriteLine("--- Switch-Case Edge Output ---");
                System.Console.WriteLine(outputEvent.As<string>());
            }
        }
    }
}
