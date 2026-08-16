using System.Text.Json.Serialization;

namespace JAV.Custom.Provider.Models;

public class RemoteActorSearch
{
    [JsonPropertyName("id")] public string Id { get; set; } = string.Empty;
    [JsonPropertyName("name")] public string Name { get; set; } = string.Empty;
    [JsonPropertyName("provider")] public string Provider { get; set; } = string.Empty;
    [JsonPropertyName("homepage")] public string Homepage { get; set; } = string.Empty;
    [JsonPropertyName("aliases")] public string[] Aliases { get; set; } = Array.Empty<string>();
    [JsonPropertyName("images")] public string[] Images { get; set; } = Array.Empty<string>();
}

public sealed class RemoteActor : RemoteActorSearch
{
    [JsonPropertyName("birthday")] public DateTime Birthday { get; set; }
    [JsonPropertyName("debut_date")] public DateTime DebutDate { get; set; }
    [JsonPropertyName("blood_type")] public string BloodType { get; set; } = string.Empty;
    [JsonPropertyName("cup_size")] public string CupSize { get; set; } = string.Empty;
    [JsonPropertyName("measurements")] public string Measurements { get; set; } = string.Empty;
    [JsonPropertyName("nationality")] public string Nationality { get; set; } = string.Empty;
    [JsonPropertyName("height")] public int Height { get; set; }
    [JsonPropertyName("hobby")] public string Hobby { get; set; } = string.Empty;
    [JsonPropertyName("skill")] public string Skill { get; set; } = string.Empty;
    [JsonPropertyName("summary")] public string Summary { get; set; } = string.Empty;
    [JsonPropertyName("place_of_birth")] public string PlaceOfBirth { get; set; } = string.Empty;
    [JsonPropertyName("original_name")] public string OriginalName { get; set; } = string.Empty;
    [JsonPropertyName("external_ids")] public Dictionary<string, string> ExternalIds { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    [JsonPropertyName("urls")] public Dictionary<string, string> Urls { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}

public sealed class ApiEnvelope<T>
{
    [JsonPropertyName("data")] public T? Data { get; set; }
}
