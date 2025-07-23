using Microsoft.Extensions.VectorData;

namespace Chronology.Models;

public class KnowledgeBaseSnippet
{
    [VectorStoreKey] public required string Key { get; set; }
    [VectorStoreData] public string? Text { get; set; }
    [VectorStoreData] public string? ReferenceDescription { get; set; }
    [VectorStoreData] public string? ReferenceLink { get; set; }
    [VectorStoreVector(1536)] public ReadOnlyMemory<float> TextEmbedding { get; set; }
}
