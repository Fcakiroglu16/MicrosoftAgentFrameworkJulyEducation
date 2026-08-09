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

        List<ChatMessage> result = await ExecuteAsync(
            "5 adet Defter sipariş etmek istiyorum.",
            (executorId, text) =>
            {
                System.Console.WriteLine();
                System.Console.Write($"{executorId}: ");
            },
            text => System.Console.Write(text));

        System.Console.WriteLine();
        System.Console.WriteLine();
        System.Console.WriteLine("--- Final Sonuç ---");
        foreach (var message in result)
            System.Console.WriteLine($"{message.Role}: {message.Text}");
    }

    public static Task<List<ChatMessage>> ExecuteAsync(string orderRequest)
        => ExecuteAsync(orderRequest, onExecutorChanged: null, onTextChunk: null);

    private static async Task<List<ChatMessage>> ExecuteAsync(
        string orderRequest,
        Action<string, string>? onExecutorChanged,
        Action<string>? onTextChunk)
    {
        var orderAgent = OrchestrationAgents.GetOrderIntakeAgent();
        var stockCheckAgent = OrchestrationAgents.GetStockCheckAgent();
        var invoiceAgent = OrchestrationAgents.GetInvoiceAgent();

        var workflow = AgentWorkflowBuilder.BuildSequential([orderAgent, stockCheckAgent, invoiceAgent]);

        var messages = new List<ChatMessage> { new(ChatRole.User, orderRequest) };

        await using StreamingRun run = await InProcessExecution.RunStreamingAsync(workflow, messages);
        await run.TrySendMessageAsync(new TurnToken(emitEvents: true));

        string? lastExecutorId = null;
        List<ChatMessage> result = new();
        await foreach (WorkflowEvent evt in run.WatchStreamAsync())
        {
            if (evt is AgentResponseUpdateEvent e)
            {
                if (e.ExecutorId != lastExecutorId)
                {
                    lastExecutorId = e.ExecutorId;
                    onExecutorChanged?.Invoke(e.ExecutorId, e.Update.Text ?? string.Empty);
                }

                onTextChunk?.Invoke(e.Update.Text ?? string.Empty);
            }
            else if (evt is WorkflowOutputEvent outputEvt)
            {
                result = outputEvt.As<List<ChatMessage>>()!;
                break;
            }
        }

        return result;
    }
}
