using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using static Qdrant.Client.Grpc.Conditions;

namespace SemanticSearch;

public static class QdrantSentences
{
    private const string _collectionName = "test_collection";
    
    internal static async Task Run()
    {
        // The C# client uses Qdrant's gRPC interface
        QdrantClient client = new QdrantClient("localhost", 6334);
        string collectionNameDot = $"collectionNameDot";
        string collectionNameCosine = $"collectionNameCosine";
        string collectionNameEuclid = $"collectionNameEuclid";
        string collectionNameManhattan = $"collectionNameManhattan";

        Dictionary<string, Distance> collectionDict = new Dictionary<string, Distance>()
        {
            { collectionNameDot, Distance.Dot },
            { collectionNameCosine, Distance.Cosine },
            { collectionNameEuclid, Distance.Euclid },
            { collectionNameManhattan, Distance.Manhattan }
        };
        
        bool populateData = false;
        
        if (populateData)
        {
            foreach (string collectionName in new [] {collectionNameDot, collectionNameCosine, collectionNameEuclid, collectionNameManhattan})
            {
                collectionDict.TryGetValue(collectionName, out Distance distance);
                await CreateCollection(client, collectionName, distance);
                await PopulateData(client, collectionName);
            }
        }
        
        // SEARCH
        string query = "Which city is better to visit?";
        IReadOnlyList<ScoredPoint> firstQuery = await RunQuery(client, collectionNameCosine, query);
        
        // IReadOnlyList<ScoredPoint> firstQuery = await RunQuery(client);
        // IReadOnlyList<ScoredPoint> secondQuery = await RunQueryWithFilter(client);

        var lol = "aaa";
    }

    # region GENERATE
    private static async Task CreateCollection(QdrantClient client, string collectionName,  Distance distance = Distance.Dot)
    {
        bool collectionExists = await client.CollectionExistsAsync(collectionName);

        if (!collectionExists)
        {
            await client.CreateCollectionAsync(collectionName: collectionName, 
                vectorsConfig: new VectorParams 
                {
                    // check the size which return embedding ollama model
                    Size = 384, Distance = distance
                });
        }
    }

    private static async Task<UpdateResult> PopulateData(QdrantClient client, string collectionName)
    {
        string dummyData = DummyData();
        List<ContentInfo> contentInfos = JsonSerializer.Deserialize<List<ContentInfo>>(dummyData!);
        
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator =
            new OllamaEmbeddingGenerator(new Uri("http://localhost:11434"), modelId: "all-minilm:22m");
        
        PointStruct[] dataArray = await Task.WhenAll(contentInfos.Select(async x =>
        {
            Embedding<float> embedding = await embeddingGenerator.GenerateEmbeddingAsync(x.Content);
    
            return new PointStruct
            {
                Id = Guid.NewGuid(),
                Vectors = embedding.Vector.Span.ToArray(),
                Payload =
                {
                    ["title"] = x.Title,
                    ["tags"] = x.Tag
                }
            };
        }));

        List<PointStruct> data = dataArray.ToList();

        UpdateResult operationInfo = await client.UpsertAsync(collectionName: collectionName,
            points: dataArray.ToList()
        );
        
        return operationInfo;
    }
    # endregion
    
    # region QUERY
    private static async Task<IReadOnlyList<ScoredPoint>> RunQuery(QdrantClient client, string collectionName, string query)
    {
        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator =
            new OllamaEmbeddingGenerator(new Uri("http://localhost:11434"), modelId: "all-minilm:22m");
        
        Embedding<float> embeddingQuery = await embeddingGenerator.GenerateEmbeddingAsync(query);
        
        IReadOnlyList<ScoredPoint> searchResult = await client.QueryAsync(
            collectionName: collectionName,
            query: embeddingQuery.Vector.Span.ToArray(),
            // filter: MatchKeyword("city", "London"),
            limit: 5,
            searchParams: new SearchParams { Exact = false, HnswEf = 128 },
            //payloadSelector: true
            payloadSelector: new WithPayloadSelector
            {
                Include = new PayloadIncludeSelector
                {
                    Fields = { new string[] { "title", "tag"} }
                },
                Exclude = new PayloadExcludeSelector { Fields = { new string[] { "content" } } }
            }
        );
        
        return searchResult;
    }
    # endregion


    private static string DummyData()
    {
        string validJson = "[{\"title\":\"The Joy of Exploring New Cultures\",\"content\":\"Traveling opens the door to diverse cultures, traditions, and lifestyles. Whether it's tasting spicy street food in Thailand, participating in a traditional tea ceremony in Japan, or dancing to live music in Brazil, immersing yourself in another culture broadens your perspective. These firsthand experiences help break down stereotypes and foster empathy, allowing travelers to appreciate the rich tapestry of human life around the globe.\",\"tag\":\"travel\"},{\"title\":\"Nature’s Therapy\",\"content\":\"Many travelers seek the serenity and wonder of nature. Hiking through the Swiss Alps, watching the Northern Lights in Iceland, or snorkeling in Australia's Great Barrier Reef offers a profound sense of peace and awe. These natural landscapes not only provide physical adventure but also act as a form of therapy—restoring mental clarity and relieving stress from everyday life.\",\"tag\":\"travel\"},{\"title\":\"The Freedom of Solo Travel\",\"content\":\"Traveling alone can be one of the most liberating experiences. It encourages independence, decision-making, and self-reflection. Solo travelers often discover hidden strengths and learn to trust their instincts. It’s also a great way to meet people, as being alone naturally invites interaction with locals and fellow adventurers.\",\"tag\":\"travel\"},{\"title\":\"Travel as a Learning Tool\",\"content\":\"Education doesn’t only happen in classrooms—travel is a powerful teacher. Visiting historical landmarks, walking through museums, or observing everyday life in a foreign country teaches lessons that textbooks can't capture. Each trip offers new knowledge about geography, history, politics, language, and more, making travelers more informed and open-minded global citizens.\",\"tag\":\"travel\"},{\"title\":\"The Challenges That Shape Us\",\"content\":\"Travel isn’t always smooth—missed flights, language barriers, and cultural misunderstandings are common. But these challenges often become the most memorable parts of the journey. They teach resilience, problem-solving, and patience. Overcoming these hurdles builds character and makes the reward of reaching your destination even sweeter.\",\"tag\":\"travel\"},{\"title\":\"The Evolution of the Automobile\",\"content\":\"Cars have come a long way since the invention of the first motorized vehicle in the late 19th century. From the early, crank-started models to today’s sleek, computer-driven electric vehicles, the automobile industry has been a symbol of technological progress. Innovations in safety, fuel efficiency, and design have made cars not just a mode of transportation but also a reflection of lifestyle and identity.\",\"tag\":\"car\"},{\"title\":\"Cars and Personal Freedom\",\"content\":\"Owning a car often represents independence and freedom. It gives people the ability to travel on their own schedule, explore remote areas, and avoid reliance on public transport. For many, especially in rural areas or regions with limited infrastructure, a car is more than just a convenience—it’s a necessity that connects them to work, education, and family.\",\"tag\":\"car\"},{\"title\":\"Environmental Impact and the Shift to Electric\",\"content\":\"As concerns about climate change grow, the car industry is undergoing a major shift toward sustainability. Gasoline and diesel engines contribute significantly to greenhouse gas emissions, prompting a global push for cleaner alternatives. Electric vehicles (EVs), hybrids, and advancements in renewable energy charging are becoming more popular, reflecting society's effort to balance mobility with environmental responsibility.\",\"tag\":\"car\"},{\"title\":\"Tokyo, Japan\",\"content\":\"Tokyo is the bustling capital of Japan and one of the most populous cities in the world. It blends traditional culture with cutting-edge technology—ancient temples stand near futuristic skyscrapers. As a global financial hub, Tokyo also offers world-class cuisine, fashion, and efficient public transport. Despite its size, the city is known for its cleanliness, safety, and polite residents.\",\"tag\":\"capital_city\"},{\"title\":\"Cairo, Egypt\",\"content\":\"Cairo is the capital of Egypt and the largest city in the Arab world. Located near the Nile River, it’s rich in history, with landmarks like the Pyramids of Giza and the Egyptian Museum, which houses thousands of ancient artifacts. Cairo is a lively city filled with markets, mosques, and a mix of modern and traditional lifestyles. It serves as a political, cultural, and educational center in the region.\",\"tag\":\"capital_city\"},{\"title\":\"Ottawa, Canada\",\"content\":\"Ottawa is the capital city of Canada, located in the province of Ontario. While not as large as Toronto or Montreal, Ottawa plays a crucial role in Canadian government and diplomacy. The city is known for its beautiful architecture, such as the Parliament Buildings, and for events like the Winterlude festival. Ottawa also offers a bilingual environment, with both English and French widely spoken.\",\"tag\":\"capital_city\"}]\n";
        return validJson;
    }
}

internal class ContentInfo
{
    [JsonPropertyName("title")] public string Title { get; set; }
    [JsonPropertyName("content")] public string Content { get; set; }
    [JsonPropertyName("tag")] public string Tag { get; set; }
}
