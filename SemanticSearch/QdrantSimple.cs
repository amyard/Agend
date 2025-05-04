using Qdrant.Client;
using Qdrant.Client.Grpc;
using static Qdrant.Client.Grpc.Conditions;

namespace SemanticSearch;

public static class QdrantSimple
{
    private const string _collectionName = "test_collection";
    internal static async Task Run()
    {
        // The C# client uses Qdrant's gRPC interface
        QdrantClient client = new QdrantClient("localhost", 6334);

        await CreateCollection(client);
        var populateDataResponse = await PopulateWithDummyData(client);
        IReadOnlyList<ScoredPoint> firstQuery = await RunQuery(client);
        IReadOnlyList<ScoredPoint> secondQuery = await RunQueryWithFilter(client);

        var lol = "aaa";
    }

    private static async Task CreateCollection(QdrantClient client)
    {
        bool collectionExists = await client.CollectionExistsAsync(_collectionName);

        if (!collectionExists)
        {
            await client.CreateCollectionAsync(collectionName: _collectionName, 
                vectorsConfig: new VectorParams 
                {
                    Size = 4, Distance = Distance.Dot
                });
        }
    }

    private static async Task<UpdateResult> PopulateWithDummyData(QdrantClient client)
    {
        UpdateResult operationInfo = await client.UpsertAsync(collectionName: _collectionName, points: new List<PointStruct>
        {
            new()
            {
                Id = 1,
                Vectors = new float[]
                {
                    0.05f, 0.61f, 0.76f, 0.74f
                },
                Payload = {
                    ["city"] = "Berlin"
                }
            },
            new()
            {
                Id = 2,
                Vectors = new float[]
                {
                    0.19f, 0.81f, 0.75f, 0.11f
                },
                Payload = {
                    ["city"] = "London"
                }
            },
            new()
            {
                Id = 3,
                Vectors = new float[]
                {
                    0.36f, 0.55f, 0.47f, 0.94f
                },
                Payload = {
                    ["city"] = "Moscow"
                }
            }
        });
        
        return operationInfo;
    }

    private static async Task<IReadOnlyList<ScoredPoint>> RunQuery(QdrantClient client)
    {
        IReadOnlyList<ScoredPoint> searchResult = await client.QueryAsync(
            collectionName: _collectionName,
            query: new float[] { 0.2f, 0.1f, 0.9f, 0.7f },
            limit: 3
        );
        
        return searchResult;
    }
    
    private static async Task<IReadOnlyList<ScoredPoint>> RunQueryWithFilter(QdrantClient client)
    {
        IReadOnlyList<ScoredPoint> searchResult = await client.QueryAsync(
            collectionName: _collectionName,
            query: new float[] { 0.2f, 0.1f, 0.9f, 0.7f },
            filter: MatchKeyword("city", "London"),
            limit: 3,
            payloadSelector: true
        );
        
        return searchResult;
    }
}
