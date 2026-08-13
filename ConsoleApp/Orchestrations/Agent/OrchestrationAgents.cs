using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using OpenAI;

namespace App.Console.Orchestrations.Agent;

public static class OrchestrationAgents
{
    private static readonly Dictionary<string, int> StockTable = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Kalem"] = 50,
        ["Defter"] = 10,
        ["Silgi"] = 0,
        ["Kitap"] = 25
    };

    private static IChatClient CreateChatClient()
    {
        var apiKey = Environment.GetEnvironmentVariable("OPEN_AI_KEY");
        if (string.IsNullOrWhiteSpace(apiKey))
            throw new InvalidOperationException("Please set the OPEN_AI_KEY environment variable.");

        return new OpenAIClient(apiKey)
            .GetChatClient("gpt-4o")
            .AsIChatClient();
    }


    private static string CheckStock(string productName, int quantity)
    {
        if (!StockTable.TryGetValue(productName, out var available))
            return $"'{productName}' isimli bir ürün stok sisteminde bulunamadı.";

        return available >= quantity
            ? $"Stok yeterli: '{productName}' için istenen {quantity} adet mevcut (stokta {available} adet var)."
            : $"Stok yetersiz: '{productName}' için istenen {quantity} adet karşılanamıyor (stokta yalnızca {available} adet var).";
    }


    public static ChatClientAgent GetOrderIntakeAgent()
    {
        var chatClient = CreateChatClient();

        return chatClient.AsAIAgent(new ChatClientAgentOptions
        {
            Name = "OrderIntakeAgent",
            Description = "Reads a free-text order request and extracts the product and quantity.",
            ChatOptions = new ChatOptions
            {
                Instructions = """
                               You are an order intake assistant.
                               - Read the user's order request written in natural language.
                               - Extract the product name and the requested quantity.
                               """,
                ResponseFormat = ChatResponseFormat.ForJsonSchema<OrderIntakeResult>()
            }
        });
    }


    public static ChatClientAgent GetStockCheckAgent()
    {
        var chatClient = CreateChatClient();

        return chatClient.AsAIAgent(new ChatClientAgentOptions
        {
            Name = "StockCheckAgent",
            Description = "Checks stock availability for the requested product and quantity by calling a tool.",
            ChatOptions = new ChatOptions
            {
                Instructions = """
                               You are a stock control assistant.
                               - You will receive a JSON object in the format {"Product": <product>, "Quantity": <quantity>}.
                               - You MUST call the CheckStock tool with the extracted product name and quantity to verify availability.
                               - Report the order (product and quantity) together with the stock check result returned by the tool.
                               """,
                Tools = [AIFunctionFactory.Create(CheckStock)]
            }
        });
    }


    public static ChatClientAgent GetInvoiceAgent()
    {
        var chatClient = CreateChatClient();

        return chatClient.AsAIAgent(new ChatClientAgentOptions
        {
            Name = "InvoiceAgent",
            Description = "Creates an invoice summary based on the order and stock check result.",
            ChatOptions = new ChatOptions
            {
                Instructions = """
                               You are an invoicing assistant.
                               - Read the previous agent's order and stock check result.
                               - If stock is sufficient, produce a short invoice summary including product, quantity,
                                 an assumed unit price of 100 TL, and the total price.
                               - If stock is insufficient, produce a short message explaining that the order cannot be
                                 invoiced due to insufficient stock.
                               """
            }
        });
    }
}

public sealed record OrderIntakeResult(string Product, int Quantity);