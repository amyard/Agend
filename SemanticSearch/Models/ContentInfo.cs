using System.Text.Json.Serialization;

namespace SemanticSearch.Models;

public class ContentInfo
{
    [JsonPropertyName("title")] public string Title { get; set; }
    [JsonPropertyName("content")] public string Content { get; set; }
    [JsonPropertyName("tag")] public string Tag { get; set; }
}
