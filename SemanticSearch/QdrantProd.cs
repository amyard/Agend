using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using SemanticSearch.Models;

namespace SemanticSearch;

public static class QdrantProd
{
    internal static async Task Run()
    {
        bool skip = false;
        string collectionName = "prod-work";
        
        var service = new QdrantServiceProd(
            host: "localhost",
            port: 6334,
            embeddingUri: new Uri("http://localhost:11434"),
            modelId: "all-minilm:22m"
        );

        if (!skip)
        {
            await service.CreateCollectionAsync(collectionName, Distance.Cosine);

            (string doc1, string _) = Helper.PdfDocumentHelper.ExtractFullTextFromDocument("text/report.pdf");
            (string doc2, string _) = Helper.PdfDocumentHelper.ExtractFullTextFromDocument("text/azure_load_balancer.pdf");

            List<ContentInfoProd> data =
            [
                new ContentInfoProd() {Id = 1, Title = "Report", Content = doc1},
                new ContentInfoProd() {Id = 2, Title = "Azure load balancer", Content = doc2}
            ];

            await service.PopulateCollectionAsync(collectionName, data);
        }

        while (true)
        {
            Console.WriteLine("Enter the prompt");
            var userInput = Console.ReadLine();
            
            IReadOnlyList<ScoredPoint> results = await service.QueryAsync(collectionName, userInput, 10);
            DisplayResult(results);
        }
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
    # endregion
}
