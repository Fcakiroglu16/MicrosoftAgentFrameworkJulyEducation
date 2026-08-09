using App.Console.Orchestrations.Agent;
using Microsoft.Agents.AI;
using Microsoft.Agents.AI.Workflows;
using Microsoft.Extensions.AI;

namespace App.Console.Orchestrations.Sequential;


public static class SequentialOrderWorkflow
{
    public static async Task RunAsync()
    {
        System.Console.WriteLine();
        System.Console.WriteLine("=== Sequential Orchestration: Sipariş -> Stok Kontrolü -> Fatura ===");

        // 1) Define the agents that will run one after another in the pipeline.
        var orderAgent = OrchestrationAgents.GetOrderIntakeAgent();
        var stockCheckAgent = OrchestrationAgents.GetStockCheckAgent();
        var invoiceAgent = OrchestrationAgents.GetInvoiceAgent();

        // 2) Build the sequential workflow.
        var workflow = AgentWorkflowBuilder.BuildSequential([orderAgent, stockCheckAgent, invoiceAgent]);

        // 3) Run the workflow with an initial user message.
        var messages = new List<ChatMessage> { new(ChatRole.User, "5 adet Defter sipariş etmek istiyorum.") };

        await using StreamingRun run = await InProcessExecution.RunStreamingAsync(workflow, messages);
        await run.TrySendMessageAsync(new TurnToken(emitEvents: true));

        string? lastExecutorId = null;
        List<ChatMessage> result = [];
        await foreach (WorkflowEvent evt in run.WatchStreamAsync())
        {
            if (evt is AgentResponseUpdateEvent e)
            {
                if (e.ExecutorId != lastExecutorId)
                {
                    lastExecutorId = e.ExecutorId;
                    System.Console.WriteLine();
                    System.Console.Write($"{e.ExecutorId}: ");
                }

                System.Console.Write(e.Update.Text);
            }
            else if (evt is WorkflowOutputEvent outputEvt)
            {
                result = outputEvt.As<List<ChatMessage>>()!;
                break;
            }
        }

        // 4) Display the final conversation.
        System.Console.WriteLine();
        System.Console.WriteLine();
        System.Console.WriteLine("--- Final Sonuç ---");
        foreach (var message in result)
            System.Console.WriteLine($"{message.Role}: {message.Text}");
    }
}
