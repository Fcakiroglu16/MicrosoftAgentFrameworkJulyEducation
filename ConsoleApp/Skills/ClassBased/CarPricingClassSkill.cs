using Microsoft.Agents.AI;

namespace App.Console.Skills.ClassBased;


public sealed class CarPricingClassSkill : AgentClassSkill<CarPricingClassSkill>
{
    public override AgentSkillFrontmatter Frontmatter { get; } = new(
        name: "car-pricing-skill",
        description: "Araç kiralamaları için toplam ücreti hesaplar");

    protected override string Instructions => """
        İstenen araç kategorisi için fiyatlandırmayı belirlemek için bu skill'i kullan.
        1. Günlük ücreti öğrenmek için pricing-table kaynağını kontrol et.
        2. Toplam ücreti hesaplamak için calculate-total script'ini çağır.
        """;
    
    [AgentSkillResource("pricing-table")]
    public string PricingTable => """
        | Kategori   | Günlük Ücret |
        |------------|--------------|
        | Ekonomik   | 30$          |
        | Sedan      | 45$          |
        | SUV        | 65$          |
        """;

   
    [AgentSkillScript("calculate-total")]
    private static double CalculateTotal(string category, int days)
    {
        var dailyRate = category.Trim().ToLowerInvariant() switch
        {
            "ekonomik" => 30d,
            "sedan" => 45d,
            "suv" => 65d,
            _ => throw new ArgumentException($"Bilinmeyen kategori: {category}")
        };

        return dailyRate * days;
    }
}
