using System.ComponentModel;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;

namespace ModelContextProtocol.McpServerWithStdio.Tools;

internal class TextContentTools
{

    [McpServerTool(Name = "get_motivational_quote")]
    [Description("Günün motivasyon sözünü TextContentBlock olarak döner.")]
    public static TextContentBlock GetMotivationalQuote()
    {
        var quotes = new[]
        {
            "Başarı, her gün tekrarlanan küçük çabaların toplamıdır.",
            "Güçlük, sizi kırmak için değil, güçlendirmek için gelir.",
            "Hayal kuruyorsanız, yarı yoldasınız demektir."
        };
        var quote = quotes[Random.Shared.Next(quotes.Length)];

        return new TextContentBlock { Text = $"Günün Sözü: {quote}" };
    }
}