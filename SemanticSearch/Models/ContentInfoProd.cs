using System.Text.Json.Serialization;

namespace SemanticSearch.Models;

public class ContentInfoProd
{
    [JsonPropertyName("title")] public string Title { get; set; }
    [JsonPropertyName("content")] public string Content { get; set; }
    [JsonPropertyName("id")] public int Id { get; set; }
}
