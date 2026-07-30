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
        2. Günlük ücreti istenen gün sayısıyla çarp.
        """;
    
    [AgentSkillResource("pricing-table")]
    public string PricingTable => """
        | Kategori   | Günlük Ücret |
        |------------|--------------|
        | Ekonomik   | 30$          |
        | Sedan      | 45$          |
        | SUV        | 65$          |
        """;
}
