using System.Text.Json.Serialization;

namespace App.Console.Agents.StructuredOutput;

internal sealed record CvInfo
{
    [JsonPropertyName("name")] public string? Name { get; init; }

    [JsonPropertyName("email")] public string? Email { get; init; }

    [JsonPropertyName("phone")] public string? Phone { get; init; }

    [JsonPropertyName("skills")] public List<string>? Skills { get; init; }

    [JsonPropertyName("experience")] public List<string>? Experience { get; init; }

    [JsonPropertyName("education")] public List<string>? Education { get; init; }
}