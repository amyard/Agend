using Qdrant.Client;
using Qdrant.Client.Grpc;
using Microsoft.Extensions.AI;
using System.Text.Json;
using SemanticSearch.Models;

namespace SemanticSearch;

public class QdrantService
{
    private readonly QdrantClient _client;
    private readonly IEmbeddingGenerator<string, Embedding<float>> _embeddingGenerator;
    private readonly ulong _vectorSize;
    private readonly string[] _payloadIncludeFields = ["title", "tag"];
    private readonly string[] _payloadExcludeFields = ["content"];

    // check the size which return embedding ollama model
    public QdrantService(string host, int port, Uri embeddingUri, string modelId = "all-minilm:22m", ulong vectorSize = 384)
    {
        // The C# client uses Qdrant's gRPC interface
        _client = new QdrantClient(host, port);
        _embeddingGenerator = new OllamaEmbeddingGenerator(embeddingUri, modelId);
        _vectorSize = vectorSize;
    }

    public async Task<bool> CollectionExistsAsync(string name) => await _client.CollectionExistsAsync(name);

    public async Task CreateCollectionAsync(string name, Distance distance)
    {
        if (!await CollectionExistsAsync(name))
        {
            await _client.CreateCollectionAsync(name, new VectorParams
            {
                Size = _vectorSize,
                Distance = distance
            });
        }
    }

    public async Task<UpdateResult> PopulateCollectionAsync(string collectionName, IEnumerable<ContentInfo> contents)
    {
        PointStruct[] points = await Task.WhenAll(contents.Select(async content =>
        {
            var embedding = await _embeddingGenerator.GenerateEmbeddingAsync(content.Content);
            
            return new PointStruct
            {
                Id = Guid.NewGuid(),
                Vectors = embedding.Vector.Span.ToArray(),
                Payload =
                {
                    ["title"] = content.Title,
                    ["tags"] = content.Tag
                }
            };
        }));

        return await _client.UpsertAsync(collectionName, points.ToList());
    }

    public async Task<IReadOnlyList<ScoredPoint>> QueryAsync(
        string collectionName,
        string queryText,
        ulong limit = 5,
        SearchParams? searchParams = null)
    {
        searchParams ??= new SearchParams { Exact = false, HnswEf = 128 };
        
        var embedding = await _embeddingGenerator.GenerateEmbeddingAsync(queryText);

        return await _client.QueryAsync(
            collectionName: collectionName,
            query: embedding.Vector.Span.ToArray(),
            // filter: MatchKeyword("city", "London"),
            limit: limit,
            searchParams: searchParams,
            payloadSelector: new WithPayloadSelector
            {
                Include = new PayloadIncludeSelector { Fields = { _payloadIncludeFields } },
                Exclude = new PayloadExcludeSelector { Fields = { _payloadExcludeFields } }
            });
    }
}
