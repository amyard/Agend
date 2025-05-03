using System.Numerics.Tensors;
using Microsoft.Extensions.AI;


namespace SemanticSearch;

public static class SimpleEmbedding
{
    public static async Task Run()
    {

        var blogPostTitles = new[]
        {
            "10 Hidden Gems You Must Visit Before They Get Popular",
            "How to Travel the World on a Budget",
            "The Ultimate Packing List for Stress-Free Adventures",
            "Top 5 Cities for Solo Travelers in 2025",
            "Why Slow Travel Might Change the Way You See the World",
            "Best Road Trips to Take This Summer",
            "Cultural Etiquette: What to Know Before You Go",
            "Adventure Travel: Destinations for the Brave at Heart",
            "A Foodie's Guide to Traveling Through Italy",
            "The Most Scenic Train Rides Around the World",
            "Travel Photography Tips for Beginners",
            "Eco-Friendly Travel: How to Explore Sustainably",
            "Why You Should Add Off-Season Travel to Your Bucket List",
            "Top 7 Travel Apps You Didn't Know You Needed",
            "Luxury Travel Experiences Worth the Splurge",
            "Island Hopping: Best Tropical Escapes for 2025",
            "How to Stay Healthy While Traveling Abroad",
            "The Future of Travel: Trends to Watch",
            "Family-Friendly Destinations Everyone Will Love",
            "How to Turn Every Trip Into a Life-Changing Experience",
        };

        IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator =
            new OllamaEmbeddingGenerator(new Uri("http://localhost:11434"), modelId: "all-minilm:22m");

        (string Value, Embedding<float> Embedding)[] candidateEmbeddings = await embeddingGenerator.GenerateAndZipAsync(blogPostTitles);

        while (true)
        {
            Console.WriteLine("Enter the prompt");
            var userInput = Console.ReadLine();
            var userEmbedding = await embeddingGenerator!.GenerateEmbeddingAsync(userInput);

            var topMatches = candidateEmbeddings.Select(candidate => new
                {
                    Text = candidate.Value,
                    Similarity = TensorPrimitives.CosineSimilarity(candidate.Embedding.Vector.Span, userEmbedding.Vector.Span)
                })
                .OrderByDescending(match => match.Similarity)
                .Take(3);

            foreach (var match in topMatches)
            {
                Console.WriteLine($"{match.Similarity}: {match.Text}");
            }
        }
    }
}
