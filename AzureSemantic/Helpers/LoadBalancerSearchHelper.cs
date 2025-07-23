using Azure.Search.Documents;
using Azure.Search.Documents.Models;
using AzureSemantic.Models;

namespace AzureSemantic.Helpers;

public static class LoadBalancerSearchHelper
{
    internal static async Task SingleVectorSearchEmbedding(SearchClient searchClient, string searchText, string filter = "")
    {
        ReadOnlyMemory<float> vectorizedResult = SemanticSearchHelper.GetEmbeddings(searchText);

        SearchResults<Book> response = await searchClient.SearchAsync<Book>(
            searchText,
            new SearchOptions
            {
                VectorSearch = new()
                {
                    Queries =
                    {
                        new VectorizedQuery(vectorizedResult)
                        {
                            KNearestNeighborsCount = 50, 
                            Fields = { nameof(Book.ContentVector) },
                            Exhaustive = true,
                            // Oversampling = 50, // Unhandled exception. Azure.RequestFailedException: Oversampling cannot be set on vector field 'ContentVector' because it does not have a compression defined.
                            // Weight = 0.1f
                            Weight = 1f
                        }
                    }
                },
                Filter = filter
            }).ConfigureAwait(false);

        int count = 0;
        Console.WriteLine($"\nSingle Vector Search Results:");
        await foreach (SearchResult<Book> result in response.GetResultsAsync())
        {
            count++;
            Book doc = result.Document;
            Console.WriteLine($"{doc.Id}: {doc.Name}. Score={result.Score:0.0000}");
        }
        Console.WriteLine($"Total number of search results:{count}");
    }
    
    // internal static async Task SingleHybridSearchEmbedding(SearchClient searchClient, string searchText, string filter = "")
    // {
    //     ReadOnlyMemory<float> vectorizedResult = SemanticSearchHelper.GetEmbeddings(searchText);
    //
    //     SearchResults<Book> response = await searchClient.SearchAsync<Book>(
    //         searchText,
    //         new SearchOptions
    //         {
    //             VectorSearch = new()
    //             {
    //                 Queries =
    //                 {
    //                     new VectorizedQuery(vectorizedResult)
    //                     {
    //                         KNearestNeighborsCount = 50, 
    //                         Fields = { nameof(Book.ContentVector) },
    //                         Exhaustive = true,
    //                         Oversampling = 50,
    //                         Weight = 0.1f
    //                     }
    //                 }
    //             },
    //             Filter = filter
    //         }).ConfigureAwait(false);
    //
    //     int count = 0;
    //     Console.WriteLine($"\nSimple Hybrid Search Results:");
    //     await foreach (SearchResult<Book> result in response.GetResultsAsync())
    //     {
    //         count++;
    //         Book doc = result.Document;
    //         Console.WriteLine($"{doc.Id}: {doc.Name}");
    //     }
    //     Console.WriteLine($"Total number of search results:{count}");
    // }
    
    internal static async Task SingleVectorSearchNotEmbedding(SearchClient searchClient, string searchText, string filter = "")
    {
        SearchResults<Book> response = await searchClient.SearchAsync<Book>(
            searchText,
            new SearchOptions
            {
                VectorSearch = new()
                {
                    Queries = 
                    {
                        // to use this shit you need to have AzureOpenAIVectorizer in search index
                        new VectorizableTextQuery(searchText) 
                        {
                            KNearestNeighborsCount = 50, 
                            Fields = { nameof(Book.ContentVector) },
                            Exhaustive = true,
                            // Oversampling = 50, // Unhandled exception. Azure.RequestFailedException: Oversampling cannot be set on vector field 'ContentVector' because it does not have a compression defined.
                            Weight = 0.1f 
                        } 
                    },
                },
                Filter = filter
            });

        int count = 0;
        Console.WriteLine($"\nSingle Vector Search Results (not embedding):");
        await foreach (SearchResult<Book> result in response.GetResultsAsync())
        {
            count++;
            Book doc = result.Document;
            Console.WriteLine($"{doc.Id}: {doc.Name}. Score={result.Score:0.0000}");
        }
        Console.WriteLine($"Total number of search results:{count}");
    }
    
    
    # region NEW TEST embedding
    internal static async Task FastVectorSearch(SearchClient searchClient, string searchText, ReadOnlyMemory<float> embeddingQuery, string filter = "")
    {
        SearchResults<Book> response = await searchClient.SearchAsync<Book>(
            new SearchOptions
            {
                VectorSearch = new()
                {
                    Queries = 
                    {
                        new VectorizedQuery(embeddingQuery) 
                        // new VectorizableTextQuery(searchText) 
                        {
                            KNearestNeighborsCount = 50, 
                            Fields = { nameof(Book.ContentVector) },
                            Oversampling = 5.0,  // Lower oversampling for speed
                        } 
                    },
                },
                Filter = filter
            });

        Console.WriteLine($"\nFastVectorSearch:");
        await DisplayResult(response);
    }
    
    internal static async Task BalancedVectorSearch(SearchClient searchClient, string searchText, ReadOnlyMemory<float> embeddingQuery, string searchField = nameof(Book.ContentVector2), string filter = "")
    {
        SearchResults<Book> response = await searchClient.SearchAsync<Book>(
            new SearchOptions
            {
                VectorSearch = new()
                {
                    Queries = 
                    {
                        new VectorizedQuery(embeddingQuery) 
                        // new VectorizableTextQuery(searchText) 
                        {
                            KNearestNeighborsCount = 50, 
                            Fields = { nameof(Book.ContentVector2) },
                            Oversampling = 10.0,
                        } 
                    },
                },
                Filter = filter
            });

        Console.WriteLine($"\nBalancedVectorSearch:");
        await DisplayResult(response);
    }
    
    internal static async Task ExactVectorSearch(SearchClient searchClient, string searchText, ReadOnlyMemory<float> embeddingQuery, string filter = "")
    {
        SearchResults<Book> response = await searchClient.SearchAsync<Book>(
            new SearchOptions
            {
                VectorSearch = new()
                {
                    Queries = 
                    {
                        new VectorizedQuery(embeddingQuery) 
                        // new VectorizableTextQuery(searchText) 
                        {
                            KNearestNeighborsCount = 50, 
                            Fields = { nameof(Book.ContentVector3) },
                            Exhaustive = true  // Force exact search
                        } 
                    },
                },
                Filter = filter
            });

        Console.WriteLine($"\nExactVectorSearch:");
        await DisplayResult(response);
    }
    
    internal static async Task HybridSearch(SearchClient searchClient, string searchText, ReadOnlyMemory<float> embeddingQuery, string filter = "")
    {
        SearchResults<Book> response = await searchClient.SearchAsync<Book>(
            searchText,
            new SearchOptions
            {
                VectorSearch = new()
                {
                    Queries = 
                    {
                        new VectorizedQuery(embeddingQuery) 
                        // new VectorizableTextQuery(searchText) 
                        {
                            KNearestNeighborsCount = 50, 
                            Fields = { nameof(Book.ContentVector) },
                            Weight = 0.3f
                        } 
                    },
                },
                Filter = filter
            });

        Console.WriteLine($"\nHybridSearch:");
        await DisplayResult(response);
    }

    private static async Task DisplayResult(SearchResults<Book> response)
    {
        int count = 0;
        await foreach (SearchResult<Book> result in response.GetResultsAsync())
        {
            count++;
            Book doc = result.Document;
            Console.WriteLine($"{doc.Id}: {doc.Name}. Score={result.Score:0.0000}");
        }
        Console.WriteLine($"Total number of search results:{count}");
    }

    # endregion
    
}
