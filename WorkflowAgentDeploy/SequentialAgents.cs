using Azure.AI.Projects;
using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;

namespace WorkflowAgentDeploy;

/// <summary>
///     Sequential orkestrasyonu oluşturan 3 alt agent'ı tanımlar:
///     Sipariş Alma -&gt; Stok Kontrolü -&gt; Fatura.
///     Her agent, aynı Foundry projesindeki model deployment'ı kullanır.
/// </summary>
public static class SequentialAgents
{
    private static readonly Dictionary<string, int> StockTable = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Kalem"] = 50,
        ["Defter"] = 10,
        ["Silgi"] = 0,
        ["Kitap"] = 25
    };

    private static string CheckStock(string productName, int quantity)
    {
        if (!StockTable.TryGetValue(productName, out var available))
            return $"'{productName}' isimli bir ürün stok sisteminde bulunamadı.";

        return available >= quantity
            ? $"Stok yeterli: '{productName}' için istenen {quantity} adet mevcut (stokta {available} adet var)."
            : $"Stok yetersiz: '{productName}' için istenen {quantity} adet karşılanamıyor (stokta yalnızca {available} adet var).";
    }

    public static ChatClientAgent GetOrderIntakeAgent(AIProjectClient projectClient, string deployment)
    {
        return projectClient.AsAIAgent(new ChatClientAgentOptions
        {
            Name = "OrderIntakeAgent",
            Description = "Serbest metin sipariş isteğini okur, ürün ve adet bilgisini çıkarır.",
            ChatOptions = new ChatOptions
            {
                ModelId = deployment,
                Instructions = """
                               Sen bir sipariş alma asistanısın.
                               - Kullanıcının doğal dilde yazdığı sipariş isteğini oku.
                               - Ürün adını ve istenen adedi çıkar.
                               """,
                ResponseFormat = ChatResponseFormat.ForJsonSchema<OrderIntakeResult>()
            }
        });
    }

    public static ChatClientAgent GetStockCheckAgent(AIProjectClient projectClient, string deployment)
    {
        return projectClient.AsAIAgent(new ChatClientAgentOptions
        {
            Name = "StockCheckAgent",
            Description = "Bir tool çağırarak istenen ürün ve adet için stok durumunu kontrol eder.",
            ChatOptions = new ChatOptions
            {
                ModelId = deployment,
                Instructions = """
                               Sen bir stok kontrol asistanısın.
                               - {"Product": <ürün>, "Quantity": <adet>} biçiminde bir JSON nesnesi alacaksın.
                               - Stok durumunu doğrulamak için MUTLAKA CheckStock tool'unu çağır.
                               - Siparişi (ürün ve adet) tool'dan dönen stok sonucuyla birlikte raporla.
                               """,
                Tools = [AIFunctionFactory.Create(CheckStock)]
            }
        });
    }

    public static ChatClientAgent GetInvoiceAgent(AIProjectClient projectClient, string deployment)
    {
        return projectClient.AsAIAgent(new ChatClientAgentOptions
        {
            Name = "InvoiceAgent",
            Description = "Sipariş ve stok kontrol sonucuna göre bir fatura özeti oluşturur.",
            ChatOptions = new ChatOptions
            {
                ModelId = deployment,
                Instructions = """
                               Sen bir faturalama asistanısın.
                               - Önceki agent'ın siparişini ve stok kontrol sonucunu oku.
                               - Stok yeterliyse; ürün, adet, varsayılan 100 TL birim fiyat ve toplam tutarı
                                 içeren kısa bir fatura özeti oluştur.
                               - Stok yetersizse; siparişin stok yetersizliği nedeniyle faturalandırılamadığını
                                 açıklayan kısa bir mesaj oluştur.
                               """
            }
        });
    }
}

public sealed record OrderIntakeResult(string Product, int Quantity);