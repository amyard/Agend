using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using SemanticSearch.Models;

namespace SemanticSearch;

public static class QdrantSentences
{
    internal static async Task Run()
    {
        bool skip = true;
        string collectionName = "collectionNameCosine";
        
        var service = new QdrantService(
            host: "localhost",
            port: 6334,
            embeddingUri: new Uri("http://localhost:11434"),
            modelId: "all-minilm:22m"
        );

        if (skip)
        {
            await service.CreateCollectionAsync(collectionName, Distance.Cosine);

            string dummyData = GetDummyData();
            var contents = JsonSerializer.Deserialize<List<ContentInfo>>(dummyData)!;
            await service.PopulateCollectionAsync(collectionName, contents);
        }

        string query = "Which city is better to visit?";
        IReadOnlyList<ScoredPoint> results = await service.QueryAsync(collectionName, query, 10);
        DisplayResult(results);
    }

    # region Common
    private static void DisplayResult(IReadOnlyList<ScoredPoint> result)
    {
        foreach (var point in result)
        {
            Console.WriteLine($"Score: {point.Score}");

            if (point.Payload != null)
            {
                Console.WriteLine("Payload:");
                foreach (var kvp in point.Payload.OrderByDescending(x => x.Key))
                {
                    string? valueObject = kvp.Value.HasStringValue ? kvp.Value.StringValue : kvp.Value.ToString();
                    Console.WriteLine($"  {kvp.Key}: {valueObject}");
                }
            }

            Console.WriteLine(new string('-', 30));
        }
    }
    
    private static string GetDummyData()
    {
        string validJson = "[{\"title\":\"The Joy of Exploring New Cultures\",\"content\":\"Traveling opens the door to diverse cultures, traditions, and lifestyles. Whether it's tasting spicy street food in Thailand, participating in a traditional tea ceremony in Japan, or dancing to live music in Brazil, immersing yourself in another culture broadens your perspective. These firsthand experiences help break down stereotypes and foster empathy, allowing travelers to appreciate the rich tapestry of human life around the globe.\",\"tag\":\"travel\"},{\"title\":\"Nature’s Therapy\",\"content\":\"Many travelers seek the serenity and wonder of nature. Hiking through the Swiss Alps, watching the Northern Lights in Iceland, or snorkeling in Australia's Great Barrier Reef offers a profound sense of peace and awe. These natural landscapes not only provide physical adventure but also act as a form of therapy—restoring mental clarity and relieving stress from everyday life.\",\"tag\":\"travel\"},{\"title\":\"The Freedom of Solo Travel\",\"content\":\"Traveling alone can be one of the most liberating experiences. It encourages independence, decision-making, and self-reflection. Solo travelers often discover hidden strengths and learn to trust their instincts. It’s also a great way to meet people, as being alone naturally invites interaction with locals and fellow adventurers.\",\"tag\":\"travel\"},{\"title\":\"Travel as a Learning Tool\",\"content\":\"Education doesn’t only happen in classrooms—travel is a powerful teacher. Visiting historical landmarks, walking through museums, or observing everyday life in a foreign country teaches lessons that textbooks can't capture. Each trip offers new knowledge about geography, history, politics, language, and more, making travelers more informed and open-minded global citizens.\",\"tag\":\"travel\"},{\"title\":\"The Challenges That Shape Us\",\"content\":\"Travel isn’t always smooth—missed flights, language barriers, and cultural misunderstandings are common. But these challenges often become the most memorable parts of the journey. They teach resilience, problem-solving, and patience. Overcoming these hurdles builds character and makes the reward of reaching your destination even sweeter.\",\"tag\":\"travel\"},{\"title\":\"The Evolution of the Automobile\",\"content\":\"Cars have come a long way since the invention of the first motorized vehicle in the late 19th century. From the early, crank-started models to today’s sleek, computer-driven electric vehicles, the automobile industry has been a symbol of technological progress. Innovations in safety, fuel efficiency, and design have made cars not just a mode of transportation but also a reflection of lifestyle and identity.\",\"tag\":\"car\"},{\"title\":\"Cars and Personal Freedom\",\"content\":\"Owning a car often represents independence and freedom. It gives people the ability to travel on their own schedule, explore remote areas, and avoid reliance on public transport. For many, especially in rural areas or regions with limited infrastructure, a car is more than just a convenience—it’s a necessity that connects them to work, education, and family.\",\"tag\":\"car\"},{\"title\":\"Environmental Impact and the Shift to Electric\",\"content\":\"As concerns about climate change grow, the car industry is undergoing a major shift toward sustainability. Gasoline and diesel engines contribute significantly to greenhouse gas emissions, prompting a global push for cleaner alternatives. Electric vehicles (EVs), hybrids, and advancements in renewable energy charging are becoming more popular, reflecting society's effort to balance mobility with environmental responsibility.\",\"tag\":\"car\"},{\"title\":\"Tokyo, Japan\",\"content\":\"Tokyo is the bustling capital of Japan and one of the most populous cities in the world. It blends traditional culture with cutting-edge technology—ancient temples stand near futuristic skyscrapers. As a global financial hub, Tokyo also offers world-class cuisine, fashion, and efficient public transport. Despite its size, the city is known for its cleanliness, safety, and polite residents.\",\"tag\":\"capital_city\"},{\"title\":\"Cairo, Egypt\",\"content\":\"Cairo is the capital of Egypt and the largest city in the Arab world. Located near the Nile River, it’s rich in history, with landmarks like the Pyramids of Giza and the Egyptian Museum, which houses thousands of ancient artifacts. Cairo is a lively city filled with markets, mosques, and a mix of modern and traditional lifestyles. It serves as a political, cultural, and educational center in the region.\",\"tag\":\"capital_city\"},{\"title\":\"Ottawa, Canada\",\"content\":\"Ottawa is the capital city of Canada, located in the province of Ontario. While not as large as Toronto or Montreal, Ottawa plays a crucial role in Canadian government and diplomacy. The city is known for its beautiful architecture, such as the Parliament Buildings, and for events like the Winterlude festival. Ottawa also offers a bilingual environment, with both English and French widely spoken.\",\"tag\":\"capital_city\"}]\n";
        return validJson;
    }
    # endregion
    
}
