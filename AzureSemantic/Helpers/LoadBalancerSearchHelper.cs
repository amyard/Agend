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
                        {
                            KNearestNeighborsCount = 50, 
                            Fields = { searchField },
                            Oversampling = 10.0,
                        } 
                    },
                },
                Filter = filter
            });

        Console.WriteLine($"\nBalancedVectorSearch:");
        await DisplayResult(response);
    }
    
    internal static async Task BalancedVectorSemanticSearch(SearchClient searchClient, string searchText, ReadOnlyMemory<float> embeddingQuery, string searchField = nameof(Book.ContentVector2), string filter = "", int maxResults = 1000)
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
                            {
                                KNearestNeighborsCount = 50, 
                                Fields = { searchField },
                                Oversampling = 10.0,
                            } 
                    },
                },
                SemanticSearch = new()
                {
                    SemanticConfigurationName = "my-semantic-config",
                    QueryCaption = new(QueryCaptionType.Extractive),
                    QueryAnswer = new(QueryAnswerType.Extractive)
                },
                QueryType = SearchQueryType.Semantic,
                Filter = filter,
                Size = maxResults, // Specify the maximum number of results to return
                IncludeTotalCount = true, // Include total count of matching documents
                Skip = 0 // Start from the beginning
            });

        Console.WriteLine($"\nSemantic Hybrid Search Results:");
        
        // Display total count if available
        if (response.TotalCount.HasValue)
        {
            Console.WriteLine($"Total matching documents: {response.TotalCount.Value}");
        }
        
        await DisplayResult(response);
    }
    
    internal static async Task BalancedVectorSemanticSearchWithPagination(SearchClient searchClient, string searchText, ReadOnlyMemory<float> embeddingQuery, string searchField = nameof(Book.ContentVector2), string filter = "", int pageSize = 50, int maxTotalResults = 1000)
    {
        List<Book> allResults = new List<Book>();
        int skip = 0;
        int totalRetrieved = 0;
        bool hasMoreResults = true;

        Console.WriteLine($"\nSemantic Hybrid Search Results (Paginated):");

        while (hasMoreResults && totalRetrieved < maxTotalResults)
        {
            int currentPageSize = Math.Min(pageSize, maxTotalResults - totalRetrieved);
            
            SearchResults<Book> response = await searchClient.SearchAsync<Book>(
                searchText,
                new SearchOptions
                {
                    VectorSearch = new()
                    {
                        Queries = 
                        {
                            new VectorizedQuery(embeddingQuery) 
                                {
                                    KNearestNeighborsCount = 50, 
                                    Fields = { searchField },
                                    Oversampling = 10.0,
                                } 
                        },
                    },
                    SemanticSearch = new()
                    {
                        SemanticConfigurationName = "my-semantic-config",
                        QueryCaption = new(QueryCaptionType.Extractive),
                        QueryAnswer = new(QueryAnswerType.Extractive)
                    },
                    QueryType = SearchQueryType.Semantic,
                    Filter = filter,
                    Size = currentPageSize,
                    IncludeTotalCount = true,
                    Skip = skip
                });

            // Display total count only on first page
            if (skip == 0 && response.TotalCount.HasValue)
            {
                Console.WriteLine($"Total matching documents: {response.TotalCount.Value}");
                
                // Display semantic answers only on first page
                if (response?.SemanticSearch?.Answers != null)
                {
                    foreach (QueryAnswerResult result in response.SemanticSearch.Answers)
                    {
                        Console.WriteLine($"Key: {result.Key}. Score: {result.Score:0.0000}");
                        Console.WriteLine($"Answer Highlights: {result.Highlights}");
                        Console.WriteLine($"Answer Text: {result.Text}");
                    }
                }
            }

            int pageCount = 0;
            await foreach (SearchResult<Book> result in response.GetResultsAsync())
            {
                pageCount++;
                totalRetrieved++;
                Book doc = result.Document;
                Console.WriteLine($"{doc.Id}: {doc.Name}. Score={result.Score:0.0000}");
                allResults.Add(doc);
            }

            Console.WriteLine($"Page results: {pageCount}, Total retrieved: {totalRetrieved}");

            // Check if we have more results to fetch
            hasMoreResults = pageCount == currentPageSize && totalRetrieved < maxTotalResults;
            skip += pageSize;
        }

        Console.WriteLine($"Final total number of search results: {totalRetrieved}");
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
        // Display semantic answers
        if (response?.SemanticSearch?.Answers != null)
        {
            Console.WriteLine("\n=== SEMANTIC ANSWERS ===");
            foreach (QueryAnswerResult result in response.SemanticSearch.Answers)
            {
                Console.WriteLine($"Answer from: {result.Key}. Score: {result.Score:0.0000}");
                Console.WriteLine($"Answer Highlights: {result.Highlights}");
                Console.WriteLine($"Answer Text: {result.Text}");
                Console.WriteLine();
            }
        }
        
        Console.WriteLine("=== ALL SEARCH RESULTS ===");
        int count = 0;
        await foreach (SearchResult<Book> result in response.GetResultsAsync())
        {
            count++;
            Book doc = result.Document;
            Console.WriteLine($"{doc.Id}: {doc.Name}. Score={result.Score:0.0000}");
            
            // Display any captions available for this result
            if (result.SemanticSearch?.Captions != null && result.SemanticSearch.Captions.Count > 0)
            {
                Console.WriteLine($"  Document Captions:");
                foreach (var caption in result.SemanticSearch.Captions)
                {
                    Console.WriteLine($"    - Highlights: {caption.Highlights}");
                    Console.WriteLine($"    - Text: {caption.Text}");
                }
            }
        }
        
        Console.WriteLine($"Retrieved search results: {count}");
        
        // Display total count if available
        if (response.TotalCount.HasValue)
        {
            Console.WriteLine($"Total matching documents in index: {response.TotalCount.Value}");
        }
    }

    # endregion
    
    // You mentioned having 11 appearances of "load balancer" in page2 but getting 7 highlights. This is normal because:
    // 1.	Semantic Search ≠ Keyword Count: Semantic search doesn't just count exact matches
    // 2.	Context Matters: It looks for semantically relevant passages, not just keyword occurrences
    // 3.	Highlight Grouping: Multiple nearby occurrences might be grouped into single highlights
    // 4.	Relevance Filtering: Only the most contextually relevant matches are highlighted
    internal static async Task EnhancedSemanticSearch(SearchClient searchClient, string searchText, ReadOnlyMemory<float> embeddingQuery, string searchField = nameof(Book.ContentVector2), string filter = "", int maxResults = 1000)
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
                            {
                                KNearestNeighborsCount = 100, 
                                Fields = { searchField },
                                Oversampling = 10.0,
                                // Weight = 0.5f // Reduce vector weight to give more importance to semantic search
                                Weight = 0.9f
                            } 
                    },
                },
                SemanticSearch = new()
                {
                    SemanticConfigurationName = "my-semantic-config",
                    QueryCaption = new(QueryCaptionType.Extractive),
                    QueryAnswer = new(QueryAnswerType.Extractive)
                },
                QueryType = SearchQueryType.Semantic,
                Filter = filter,
                Size = maxResults,
                IncludeTotalCount = true,
                Skip = 0
            });

        Console.WriteLine($"\nEnhanced Semantic Search Results:");
        
        // Display total count if available
        if (response.TotalCount.HasValue)
        {
            Console.WriteLine($"Total matching documents: {response.TotalCount.Value}");
        }
        
        await DisplayEnhancedResult(response);
    }

    /// <summary>
    /// Performs a literal text search without any vector embeddings or semantic processing.
    /// Searches directly in the Content field using simple text matching.
    /// </summary>
    /// <param name="searchClient">Azure Search client</param>
    /// <param name="searchText">Text to search for literally</param>
    /// <param name="filter">Optional OData filter expression</param>
    /// <param name="maxResults">Maximum number of results to return</param>
    /// <returns>Task representing the async operation</returns>
    internal static async Task LiteralContentSearch(SearchClient searchClient, string searchText, string filter = "", int maxResults = 1000)
    {
        // searchText = searchText.Insert(searchText.Length, "~");
        // searchText = $"load balancer";
        // searchText = $"load balancer~";
        searchText = $"\"load balancer\"";
        // searchText = $"\"load balancer~\"";
        
        SearchResults<Book> response = await searchClient.SearchAsync<Book>(
            searchText,
            new SearchOptions
            {
                // Use simple query type for literal text matching
                QueryType = SearchQueryType.Simple,
                SearchMode = SearchMode.All,
                
                // Search only in the Content field for literal matches
                SearchFields = { nameof(Book.Content) },
                
                // Configure to return matching data
                Size = 1000,
                IncludeTotalCount = true,
                Skip = 0,
                
                // Enable highlighting to show where matches occur in content
                HighlightFields = { nameof(Book.Content) },
                HighlightPreTag = "<mark>",
                HighlightPostTag = "</mark>",
                
                // Apply any filters
                Filter = filter,
                
                // Include all fields in results
                Select = { "*" }
            });

        Console.WriteLine($"\nLiteral Content Search Results:");
        Console.WriteLine($"Search Query: '{searchText}'");
        
        // Display total count if available
        if (response.TotalCount.HasValue)
        {
            Console.WriteLine($"Total matching documents: {response.TotalCount.Value}");
        }
        
        await DisplayLiteralSearchResults(response, searchText);
    }
    
    private static async Task DisplayLiteralSearchResults(SearchResults<Book> response, string searchText)
    {
        Console.WriteLine("\n=== LITERAL CONTENT SEARCH RESULTS ===");
        
        int count = 0;
        var allResults = new List<Book>();

        await foreach (SearchResult<Book> result in response.GetResultsAsync())
        {
            count++;
            Book doc = result.Document;
            allResults.Add(doc);
            
            Console.WriteLine($"\n📄 {count}. {doc.Id}: {doc.Name}");
            Console.WriteLine($"   📊 Relevance Score: {result.Score:0.0000}");
            Console.WriteLine($"   📝 Category: {doc.Category}");
            
            // Display highlights from Azure Search
            if (result.Highlights?.ContainsKey(nameof(Book.Content)) == true)
            {
                IList<string>? data = result.Highlights[nameof(Book.Content)];
                string content = string.Join(" ", data);
                
                // Count actual phrase occurrences in highlights
                var hCount = 0;
                foreach (var highlight in data)
                {
                    // Count <mark> tags which indicate highlighted phrases
                    hCount += highlight.Split("<mark>", StringSplitOptions.RemoveEmptyEntries).Length - 1;
                }
                
                Console.WriteLine($"   🔍 Azure Search Highlights:");
                Console.WriteLine($"      • Highlight segments: {data.Count}. Phrase occurrences: {hCount}");
                Console.WriteLine($"      • {content}");
            }
            
            // Show exact phrase match count in content using our manual counting
            // int literalMatches = CountLiteralMatches(doc.Content, searchText);
            // Console.WriteLine($"   🎯 Exact Phrase Matches Found: {literalMatches}");
            //
            // // Show content preview with manual highlighting
            // if (!string.IsNullOrWhiteSpace(doc.Content))
            // {
            //     string highlightedContent = HighlightLiteralMatches(doc.Content, searchText);
            //     string preview = highlightedContent.Length > 500 
            //         ? highlightedContent[..500] + "..." 
            //         : highlightedContent;
            //     Console.WriteLine($"   📄 Content Preview: {preview}");
            // }
            //
            // // Show full content length for reference
            // Console.WriteLine($"   📋 Full Content Available: {doc.Content?.Length ?? 0} characters");
        }
        
        // Console.WriteLine($"\n📊 EXACT PHRASE SEARCH SUMMARY");
        // Console.WriteLine($"📈 Total Documents Found: {count}");
        // Console.WriteLine($"📊 Search completed for exact phrase: '{searchText}'");
        //
        // // Calculate and display comprehensive literal match statistics
        // await DisplayLiteralMatchStatistics(allResults, searchText);
    }
    
    private static async Task DisplayLiteralMatchStatistics(List<Book> results, string searchText)
    {
        if (!results.Any())
        {
            Console.WriteLine("No results to analyze.");
            return;
        }
        
        Console.WriteLine($"\n📊 LITERAL MATCH STATISTICS");
        
        int totalMatches = 0;
        var matchesByDocument = new Dictionary<string, int>();
        var categoryCounts = new Dictionary<string, int>();
        
        foreach (var doc in results)
        {
            int docMatches = CountLiteralMatches(doc.Content, searchText);
            totalMatches += docMatches;
            matchesByDocument[doc.Id] = docMatches;
            
            // Count by category
            if (!string.IsNullOrEmpty(doc.Category))
            {
                categoryCounts[doc.Category] = categoryCounts.GetValueOrDefault(doc.Category, 0) + docMatches;
            }
        }
        
        Console.WriteLine($"🎯 Total Literal Matches Across All Documents: {totalMatches}");
        Console.WriteLine($"📊 Average Matches Per Document: {(results.Count > 0 ? (double)totalMatches / results.Count : 0):0.2f}");
        
        Console.WriteLine($"\n🏆 TOP DOCUMENTS BY LITERAL MATCH COUNT:");
        var topDocs = matchesByDocument.OrderByDescending(kvp => kvp.Value).Take(5);
        foreach (var kvp in topDocs)
        {
            var doc = results.FirstOrDefault(r => r.Id == kvp.Key);
            Console.WriteLine($"   {kvp.Key}: {kvp.Value} matches - {doc?.Name}");
        }
        
        Console.WriteLine($"\n📊 MATCHES BY CATEGORY:");
        foreach (var kvp in categoryCounts.OrderByDescending(c => c.Value))
        {
            Console.WriteLine($"   {kvp.Key}: {kvp.Value} total matches");
        }
        
        Console.WriteLine($"\n🔍 SEARCH TERM ANALYSIS:");
        Console.WriteLine($"   Search Term: '{searchText}'");
        Console.WriteLine($"   Term Length: {searchText.Length} characters");
        Console.WriteLine($"   Case Sensitive: No (search is case-insensitive)");
        Console.WriteLine($"   Match Type: Literal string matching in Content field");
    }
    
    private static int CountLiteralMatches(string content, string searchTerm)
    {
        if (string.IsNullOrEmpty(content) || string.IsNullOrEmpty(searchTerm))
            return 0;
            
        // Case-insensitive literal string matching
        int count = 0;
        int index = 0;
        string lowerContent = content.ToLowerInvariant();
        string lowerSearchTerm = searchTerm.ToLowerInvariant();
        
        while ((index = lowerContent.IndexOf(lowerSearchTerm, index)) != -1)
        {
            count++;
            index += lowerSearchTerm.Length;
        }
        
        return count;
    }
    
    private static string HighlightLiteralMatches(string content, string searchTerm)
    {
        if (string.IsNullOrEmpty(content) || string.IsNullOrEmpty(searchTerm))
            return content ?? string.Empty;
            
        // Case-insensitive replacement with highlighting
        return content.Replace(searchTerm, $"**{searchTerm}**", StringComparison.OrdinalIgnoreCase);
    }

    private static async Task DisplayEnhancedResult(SearchResults<Book> response)
    {
        // Display semantic answers with more detail
        if (response?.SemanticSearch?.Answers != null)
        {
            Console.WriteLine("\n=== SEMANTIC ANSWERS FROM ALL DOCUMENTS ===");
            foreach (QueryAnswerResult result in response.SemanticSearch.Answers)
            {
                Console.WriteLine($"📄 Document: {result.Key} | Relevance Score: {result.Score:0.0000} | Amount of search: {result.Highlights.Split("<em>", StringSplitOptions.RemoveEmptyEntries).Count()}");
                Console.WriteLine($"🔍 Highlighted Answer: {result.Highlights}");
                // Console.WriteLine($"📝 Full Answer: {result.Text}");
                Console.WriteLine(new string('-', 80));
            }
        }
        else
        {
            Console.WriteLine("No semantic answers found.");
        }

        Console.WriteLine("\n=== ALL DOCUMENT RESULTS WITH SEMANTIC INFORMATION ===");
        int count = 0;
        int amountOfSearchGeneral = 0;
        await foreach (SearchResult<Book> result in response.GetResultsAsync())
        {
            count++;
            Book doc = result.Document;
            Console.WriteLine($"\n📄 {count}. {doc.Id}: {doc.Name}");
            Console.WriteLine($"   📊 Relevance Score: {result.Score:0.0000}");
            // Console.WriteLine($"   📝 Content Preview: {doc.Content[..Math.Min(200, doc.Content.Length)]}...");
            
            // Display semantic captions for this specific result if available
            if (result.SemanticSearch?.Captions != null && result.SemanticSearch.Captions.Count > 0)
            {
                Console.WriteLine($"   🎯 Semantic Captions:");
                foreach (var caption in result.SemanticSearch.Captions)
                {
                    int amountOfSearch = (caption.Highlights ?? caption.Text).Split("<em>", StringSplitOptions.RemoveEmptyEntries).Count();
                    amountOfSearchGeneral += amountOfSearch;
                    Console.WriteLine($"      • Amount of search: {amountOfSearch}");
                    Console.WriteLine($"      • {caption.Highlights ?? caption.Text}");
                }
            }
        }
        
        Console.WriteLine($"\n📊 Summary: amountOfSearch {amountOfSearchGeneral}");
        Console.WriteLine($"\n📊 Summary: Retrieved {count} search results");
        
        // Display total count if available
        if (response.TotalCount.HasValue)
        {
            Console.WriteLine($"📈 Total matching documents in index: {response.TotalCount.Value}");
        }
    }
}
