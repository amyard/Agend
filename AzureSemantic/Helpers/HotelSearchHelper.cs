using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using AzureSemantic.Models;

namespace AzureSemantic.Helpers;

public static class HotelSearchHelper
{
    internal static async Task SingleVectorSearchEmbedding(SearchClient searchClient, string searchText)
    {
        ReadOnlyMemory<float> vectorizedResult = SemanticSearchHelper.GetEmbeddings(searchText);

        SearchResults<Hotel> response = await searchClient.SearchAsync<Hotel>(
            new SearchOptions
            {
                VectorSearch = new()
                {
                    Queries = { new VectorizedQuery(vectorizedResult) { KNearestNeighborsCount = 3, Fields = { "DescriptionVector" } } }
                }
            }).ConfigureAwait(false);

        int count = 0;
        Console.WriteLine($"Single Vector Search Results:");
        await foreach (SearchResult<Hotel> result in response.GetResultsAsync())
        {
            count++;
            Hotel doc = result.Document;
            Console.WriteLine($"{doc.HotelId}: {doc.HotelName}");
        }
        Console.WriteLine($"Total number of search results:{count}");
    }
    
    internal static async Task SingleHybridSearchEmbedding(SearchClient searchClient, string searchText)
    {
        ReadOnlyMemory<float> vectorizedResult = SemanticSearchHelper.GetEmbeddings(searchText);

        SearchResults<Hotel> response = await searchClient.SearchAsync<Hotel>(
            searchText,
            new SearchOptions
            {
                VectorSearch = new()
                {
                    Queries =
                    {
                        new VectorizedQuery(vectorizedResult)
                        {
                            KNearestNeighborsCount = 3, 
                            Fields = { "DescriptionVector" }
                        }
                    }
                }
            }).ConfigureAwait(false);

        int count = 0;
        Console.WriteLine($"\nSimple Hybrid Search Results:");
        await foreach (SearchResult<Hotel> result in response.GetResultsAsync())
        {
            count++;
            Hotel doc = result.Document;
            Console.WriteLine($"{doc.HotelId}: {doc.HotelName}");
        }
        Console.WriteLine($"Total number of search results:{count}");
    }
    
    internal static async Task SingleVectorSearchNotEmbedding(SearchClient searchClient, string searchText)
    {
        SearchResults<Hotel> response = await searchClient.SearchAsync<Hotel>(
            new SearchOptions
            {
                VectorSearch = new()
                {
                    Queries = 
                    {
                        // to use this shit you need to have AzureOpenAIVectorizer in search index
                        new VectorizableTextQuery(searchText) 
                        {
                            KNearestNeighborsCount = 3,
                            Fields = { "DescriptionVector" } 
                        } 
                    },
                }
            });

        int count = 0;
        Console.WriteLine($"Single Vector Search Results (not embedding):");
        await foreach (SearchResult<Hotel> result in response.GetResultsAsync())
        {
            count++;
            Hotel doc = result.Document;
            Console.WriteLine($"{doc.HotelId}: {doc.HotelName}");
        }
        Console.WriteLine($"Total number of search results:{count}");
    }
    
}
