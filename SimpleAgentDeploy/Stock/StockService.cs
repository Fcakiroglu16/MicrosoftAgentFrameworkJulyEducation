namespace SimpleAgentDeploy.Stock;

public class StockService
{
    private readonly Dictionary<string, int> _stockTable = new(StringComparer.OrdinalIgnoreCase)
    {
        ["Kalem"] = 50,
        ["Defter"] = 10,
        ["Silgi"] = 0,
        ["Kitap"] = 25
    };

    public string CheckStock(string productName, int quantity)
    {
        if (!_stockTable.TryGetValue(productName, out var available))
            return $"'{productName}' isimli bir ürün stok sisteminde bulunamadı.";

        return available >= quantity
            ? $"Stok yeterli: '{productName}' için istenen {quantity} adet mevcut (stokta {available} adet var)."
            : $"Stok yetersiz: '{productName}' için istenen {quantity} adet karşılanamıyor (stokta yalnızca {available} adet var).";
    }
}
