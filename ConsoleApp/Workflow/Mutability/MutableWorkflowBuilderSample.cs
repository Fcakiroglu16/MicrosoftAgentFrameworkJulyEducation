using App.Console.Workflow.Executors;
using Microsoft.Agents.AI.Workflows;

namespace App.Console.Workflow.Mutability;


public static class MutableWorkflowBuilderSample
{
    private const string Input = "hello from the mutable builder sample";

    public static async Task Run()
    {
        var upperCaseExecutor = new UpperCaseExecutor();
        var truncateExecutor = new TruncateExecutor(maxLength: 15);
        var exclamationExecutor = new ExclamationExecutor();
        var builder = new WorkflowBuilder(upperCaseExecutor);
        
        builder.AddEdge(upperCaseExecutor, truncateExecutor);

        System.Console.WriteLine("Builder created and first edge added (UpperCase -> Truncate).");


        builder.AddEdge(truncateExecutor, exclamationExecutor);
        builder.WithOutputFrom(exclamationExecutor);

        System.Console.WriteLine("Builder mutated again (Truncate -> Exclamation) before Build().");

     
        var workflow = builder.Build();

        System.Console.WriteLine("Workflow built - from this point on the graph is frozen (immutable).");

        var run = await InProcessExecution.RunAsync(workflow, Input);

        foreach (var workflowEvent in run.NewEvents)
        {
            if (workflowEvent is WorkflowOutputEvent outputEvent)
            {
                System.Console.WriteLine("--- Mutable Builder Sample Output ---");
                System.Console.WriteLine(outputEvent.As<string>());
            }
        }
    }
}
