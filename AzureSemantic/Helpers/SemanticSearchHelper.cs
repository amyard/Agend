using System.Text.Json;
using Azure;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.AI.OpenAI;
using Azure.Search.Documents;
using AzureSemantic.Models;
using OpenAI.Embeddings;

namespace AzureSemantic.Helpers;

public static class SemanticSearchHelper
{
    private static string _searchEndpoint;
    private static string _searchApiKey;
    private static string _openAiEndpoint;
    private static string _openAiKey;

    private static string _embeddingModel = "text-embedding-3-small";
    private static int _modelDimensions = 1032;
    private static string _vectorSearchProfileName = "my-vector-profile";
    private static string _vectorSearchHnswConfig = "my-hsnw-vector-config";
    private static string _vectorizerName = "openai";

    static SemanticSearchHelper()
    {
        string jsonString = File.ReadAllText("Data/secrets.json");
        Settings settings = JsonSerializer.Deserialize<Settings>(jsonString);
        
        _searchEndpoint = settings.SearchEndpoint;
        _searchApiKey = settings.SearchApiKey;
        _openAiEndpoint = settings.OpenAiEndpoint;
        _openAiKey = settings.OpenAiKey;
    }
    
    #region GENERAL
    internal static SearchIndexClient GetSearchIndexClient()
    {
        Uri endpoint = new(_searchEndpoint);
        AzureKeyCredential credential = new(_searchApiKey);
        SearchIndexClient indexClient = new(endpoint, credential);
        
        return indexClient;
    }

    internal static SearchClient GetSearchClient(string indexName)
    {
        SearchClient searchClient = new(new Uri(_searchEndpoint), indexName, new AzureKeyCredential(_searchApiKey));
        
        return searchClient;
    }
    
    public static ReadOnlyMemory<float> GetEmbeddings(string input)
    {
        Uri endpoint = new Uri(_openAiEndpoint);
        AzureKeyCredential credential = new AzureKeyCredential(_openAiKey);

        AzureOpenAIClient openAIClient = new AzureOpenAIClient(endpoint, credential);
        EmbeddingClient embeddingClient = openAIClient.GetEmbeddingClient(_embeddingModel);
        EmbeddingGenerationOptions embeddingOptions = new() { Dimensions = _modelDimensions };

        OpenAIEmbedding embedding = embeddingClient.GenerateEmbedding(input, embeddingOptions);
        return embedding.ToFloats();
    }

    public static EmbeddingClient GetEmbeddingClient()
    {
        Uri endpoint = new Uri(_openAiEndpoint);
        AzureKeyCredential credential = new AzureKeyCredential(_openAiKey);

        AzureOpenAIClient openAIClient = new AzureOpenAIClient(endpoint, credential);
        EmbeddingClient embeddingClient = openAIClient.GetEmbeddingClient(_embeddingModel);
        
        return embeddingClient;
    }
    
    public static float[] GetEmbeddings(EmbeddingClient embeddingClient, string input)
    {
        EmbeddingGenerationOptions embeddingOptions = new() { Dimensions = _modelDimensions };
        OpenAIEmbedding embedding = embeddingClient.GenerateEmbedding(input, embeddingOptions);
        
        return embedding.ToFloats().ToArray();
    }
    #endregion

    #region HOTEL
    internal static SearchIndex PrepareHotelSearchIndex(string indexName)
    {
        SearchIndex searchIndex = new(indexName)
        {
            Fields =
            {
                new SimpleField("HotelId", SearchFieldDataType.String) { IsKey = true, IsFilterable = true, IsSortable = true, IsFacetable = true },
                new SearchableField("HotelName") { IsFilterable = true, IsSortable = true },
                new SearchableField("Description") { IsFilterable = true },
                new VectorSearchField("DescriptionVector", _modelDimensions, _vectorSearchProfileName),
                new SearchableField("Category") { IsFilterable = true, IsSortable = true, IsFacetable = true },
                new VectorSearchField("CategoryVector", _modelDimensions, _vectorSearchProfileName),
            },
            VectorSearch = new()
            {
                Profiles =
                {
                    new VectorSearchProfile(_vectorSearchProfileName, _vectorSearchHnswConfig)
                    {
                        VectorizerName = _vectorizerName
                    }
                },
                Algorithms =
                {
                    new HnswAlgorithmConfiguration(_vectorSearchHnswConfig)
                },
                Vectorizers =
                {
                    new AzureOpenAIVectorizer(_vectorizerName)
                    {
                        Parameters  = new AzureOpenAIVectorizerParameters()
                        {
                            ResourceUri = new Uri(_openAiEndpoint),
                            ApiKey = _openAiKey,
                            DeploymentName = _embeddingModel,
                            ModelName = AzureOpenAIModelName.TextEmbedding3Small
                        }
                    }
                }
            },
        };

        return searchIndex;
    }
    
    public static Hotel[] GetHotelDocuments()
    {
        return new[]
        {
            new Hotel()
            {
                HotelId = "1",
                HotelName = "Fancy Stay",
                Description =
                    "Best hotel in town if you like luxury hotels. They have an amazing infinity pool, a spa, " +
                    "and a really helpful concierge. The location is perfect -- right downtown, close to all " +
                    "the tourist attractions. We highly recommend this hotel.",
                DescriptionVector = GetEmbeddings(
                    "Best hotel in town if you like luxury hotels. They have an amazing infinity pool, a spa, " +
                    "and a really helpful concierge. The location is perfect -- right downtown, close to all " +
                    "the tourist attractions. We highly recommend this hotel."),
                Category = "Luxury",
                CategoryVector = GetEmbeddings("Luxury")
            },
            new Hotel()
            {
                HotelId = "2",
                HotelName = "Roach Motel",
                Description = "Cheapest hotel in town. Infact, a motel.",
                DescriptionVector = GetEmbeddings("Cheapest hotel in town. Infact, a motel."),
                Category = "Budget",
                CategoryVector = GetEmbeddings("Budget")
            }
        };
    }
    #endregion
    
    #region LOAD BALANCER
    // internal static SearchIndex PrepareLoadBalancerSearchIndex(string indexName)
    // {
    //     SearchIndex searchIndex = new(indexName)
    //     {
    //         Fields =
    //         {
    //             new SimpleField("Id", SearchFieldDataType.String) { IsKey = true, IsFilterable = true, IsSortable = true, IsFacetable = true },
    //             new SearchableField("Name") { IsFilterable = true, IsSortable = true },
    //             new SearchableField("Category") { IsFilterable = true, IsSortable = true, IsFacetable = true },
    //             new SearchableField("Content") { IsFilterable = true },
    //             new VectorSearchField("ContentVector", _modelDimensions, _vectorSearchProfileName),
    //         },
    //         VectorSearch = new()
    //         {
    //             Profiles =
    //             {
    //                 new VectorSearchProfile(_vectorSearchProfileName, _vectorSearchHnswConfig)
    //                 {
    //                     VectorizerName = _vectorizerName
    //                 }
    //             },
    //             Algorithms =
    //             {
    //                 new HnswAlgorithmConfiguration(_vectorSearchHnswConfig)
    //             },
    //             Vectorizers =
    //             {
    //                 new AzureOpenAIVectorizer(_vectorizerName)
    //                 {
    //                     Parameters  = new AzureOpenAIVectorizerParameters()
    //                     {
    //                         ResourceUri = new Uri(_openAiEndpoint),
    //                         ApiKey = _openAiKey,
    //                         DeploymentName = _embeddingModel,
    //                         ModelName = AzureOpenAIModelName.TextEmbedding3Small
    //                     }
    //                 }
    //             }
    //         },
    //     };
    //
    //     return searchIndex;
    // }
    
    internal static SearchIndex PrepareLoadBalancerSearchIndex(string indexName)
    {
        SearchIndex searchIndex = new(indexName)
        {
            Fields =
            {
                new SimpleField("Id", SearchFieldDataType.String) { IsKey = true, IsFilterable = true, IsSortable = true, IsFacetable = true },
                new SearchableField("Name") { IsFilterable = true, IsSortable = true },
                new SearchableField("Category") { IsFilterable = true, IsSortable = true, IsFacetable = true },
                new SearchableField("Content") { IsFilterable = true },
                new VectorSearchField("ContentVector", _modelDimensions, "fast-profile"),
                new VectorSearchField("ContentVector2", _modelDimensions, "balanced-profile"),
                new VectorSearchField("ContentVector3", _modelDimensions, "exact-profile"),
                new SearchField("ContentVector4", SearchFieldDataType.Collection(SearchFieldDataType.Single))
                {
                    IsSearchable = true,
                    VectorSearchDimensions = _modelDimensions,
                    VectorSearchProfileName = "balanced-profile"
                }
            },
            VectorSearch = new()
            {
                Compressions =
                {
                    new ScalarQuantizationCompression("my-scalar-quantization")
                    {
                        RerankWithOriginalVectors = true,
                        DefaultOversampling = 10.0,
                        // ScalarQuantizationParameters = new ScalarQuantizationParameters
                        // {
                        //     QuantizedDataType = "int8"
                        // }

                    }
                },
                Profiles =
                {
                    // Fast but less accurate
                    new VectorSearchProfile("fast-profile", "hnsw-1")
                    {
                        VectorizerName = _vectorizerName,
                        CompressionName = "my-scalar-quantization"
                    },
                
                    // Balanced accuracy and speed
                    new VectorSearchProfile("balanced-profile", "hnsw-2")
                    {
                        VectorizerName = _vectorizerName,
                        CompressionName = "my-scalar-quantization"
                    },
                
                    // Most accurate but slower
                    new VectorSearchProfile("exact-profile", "eknn")
                    {
                        VectorizerName = _vectorizerName
                        // No compression for exact search
                    }
                },
                Algorithms =
                {
                    new HnswAlgorithmConfiguration("hnsw-1")
                    {
                        Parameters = new HnswParameters
                        {
                            M = 4,              // Fewer connections (faster, less accurate)
                            EfConstruction = 200, // Lower construction quality
                            EfSearch = 200,      // Lower search quality
                            Metric = VectorSearchAlgorithmMetric.Cosine
                            // Metric = VectorSearchAlgorithmMetric.DotProduct
                        }
                    },
                
                    // Balanced configuration
                    new HnswAlgorithmConfiguration("hnsw-2")
                    {
                        Parameters = new HnswParameters
                        {
                            M = 8,              // Moderate connections
                            EfConstruction = 400,
                            EfSearch = 400,
                            Metric = VectorSearchAlgorithmMetric.Cosine
                            // Metric = VectorSearchAlgorithmMetric.Euclidean
                        }
                    },
                
                    // Exact configuration
                    new ExhaustiveKnnAlgorithmConfiguration("eknn")
                    {
                        Parameters = new ExhaustiveKnnParameters
                        {
                            Metric = VectorSearchAlgorithmMetric.Cosine
                        }
                    }
                },
                Vectorizers =
                {
                    new AzureOpenAIVectorizer(_vectorizerName)
                    {
                        Parameters = new AzureOpenAIVectorizerParameters()
                        {
                            ResourceUri = new Uri(_openAiEndpoint),
                            ApiKey = _openAiKey,
                            DeploymentName = _embeddingModel,
                            ModelName = AzureOpenAIModelName.TextEmbedding3Small
                        }
                    }
                }
            }
        };

        return searchIndex;
    }
    
    public static Book[] GetLoadBalancerDocuments()
    {
        string page1_1 = "10 Load balancers";
        string page1_2 = "Load balancers are used to support scaling and high availability for applications and services. A load balancer is primarily composed of three components—a frontend, a backend, and routing rules. Requests coming to the frontend of a load balancer are distributed based on routing rules to the backend, where we place multiple instances of a service. This can be used for performance-related reasons, where we would like to distribute trafﬁc equally between endpoints in the backend, or for high availability, where multiple instances of services are used to increase the chances that at least one endpoint will be available at all times. We will cover the following recipes in this chapter: • Creating an internal load balancer • Creating a public load balancer • Creating a backend pool • Creating health probes • Creating load balancer rules • Creating inbound NAT rules • Creating explicit outbound rules";
        
        string page2_1 = "176 | Load balancers";
        string page2_2 = "Technical requirements For this chapter, an Azure subscription is required. The code samples can be found at https://github.com/PacktPublishing/Azure- Networking-Cookbook-Second-Edition/tree/master/Chapter10. Creating an internal load balancer Microsoft Azure supports two types of load balancers—internal and public. An internal load balancer is assigned a private IP address (from the address range of subnets in the virtual network) for a frontend IP address, and it targets the private IP addresses of our services (usually, an Azure virtual machine (VM)) in the backend. An internal load balancer is usually used by services that are not internet-facing and are accessed only from within our virtual network.";
        string page2_3 = "Getting ready Before you start, open the browser and go to the Azure portal via https://portal.azure. com.";
        string page2_4 = "How to do it... In order to create a new internal load balancer with the Azure portal, we must use the following steps: 1. In the Azure portal, select Create a resource and choose Load Balancer under Networking services (or search for Load Balancer in the search bar). 2. In the new pane, we must select a Subscription option and a Resource group option for where the load balancer is to be created. Then, we must provide information for the Name, Region, Type, and SKU options. In this case, we select Internal for Type to deploy an internal load balancer and set SKU to Standard. Finally, we must select the Virtual network and the Subnet that the load balancer will be associated with, along with information about the IP address assignment, which can be Static or Dynamic:";
        
        string page3_1 = "Creating an internal load balancer | 177";
        string page3_2 = "Figure 10.1: Creating a new internal load balancer 3. After all the information is entered, we select the Review + create option to validate the information and start the deployment of the load balancer.";
        
        string page4_1 = "178 | Load balancers";
        string page4_2 = "How it works... An internal load balancer is assigned a private IP address, and all requests coming to the frontend of an internal load balancer must come to that private address. This limits the trafﬁc coming to the load balancer to be from within the virtual network associated with the load balancer. Trafﬁc can come from other networks (other virtual networks or local networks) if there is some kind of virtual private network (VPN) in place. The trafﬁc coming to the frontend of the internal load balancer will be distributed across the endpoints in the backend of the load balancer. Internal load balancers are usually used for services that are not placed in a demilitarized zone (DMZ) (and are therefore not accessible over the internet), but rather in middle- or back-tier services in a multi- tier application architecture. We also need to keep in mind the differences between the Basic and Standard SKUs. The main difference is in performance (this is better in the Standard SKU) and SLA (Standard has an SLA guaranteeing 99.99% availability, while Basic has no SLA). Also, note that Standard SKU requires a Network Security Group (NSG). If an NSG is not present on the subnet or Network Interface, or NIC (of the VM in the backend), trafﬁc will not be allowed to reach its target. For more information on load balancer SKUs, see https://docs.microsoft.com/azure/load-balancer/skus. Creating a public load balancer The second type of load balancer in Azure is a public load balancer. The main difference is that a public load balancer is assigned a public IP address in the frontend, and all requests come over the internet. The requests are then distributed to the endpoints in the backend.";
        string page4_3 = "Getting ready Before you start, open the browser and go to the Azure portal via https://portal.azure. com.";
        string page4_4 = "How to do it... In order to create a new public load balancer with the Azure portal, we must follow these steps: 1. In the Azure portal, select Create a resource and choose Load Balancer under Networking services (or search for Load Balancer in the search bar).";
        
        
        string page1 = string.Join("\n", page1_1, page1_2);
        string page2 = string.Join("\n", page2_1, page2_2, page2_3, page2_4);
        string page3 = string.Join("\n", page3_1, page3_2);
        string page4 = string.Join("\n", page4_1, page4_2, page4_3);
        
        string allPages = string.Join("\n", page1, page2, page3, page4);
        
        ReadOnlyMemory<float> vectorPage1 = GetEmbeddings(page1);
        ReadOnlyMemory<float> vectorPage2 = GetEmbeddings(page2);
        ReadOnlyMemory<float> vectorPage3 = GetEmbeddings(page3);
        ReadOnlyMemory<float> vectorPage4 = GetEmbeddings(page4);
        
        return new[]
        {
            // new Book() { Id = "allPages", Category = "doc", Name = "Load Balancer P.1-5", Content = allPages, ContentVector = GetEmbeddings(allPages) },
            
            new Book() { Id = "page1", Category = "pages", Name = "Load Balancer P.1", Content = page1, ContentVector = vectorPage1, ContentVector2 = vectorPage1, ContentVector3 = vectorPage1, ContentVector4 = vectorPage1 },
            new Book() { Id = "page2", Category = "pages", Name = "Load Balancer P.2", Content = page2, ContentVector = vectorPage2, ContentVector2 = vectorPage2, ContentVector3 = vectorPage2, ContentVector4 = vectorPage2 },
            new Book() { Id = "page3", Category = "pages", Name = "Load Balancer P.3", Content = page3, ContentVector = vectorPage3, ContentVector2 = vectorPage3, ContentVector3 = vectorPage3, ContentVector4 = vectorPage3 },
            new Book() { Id = "page4", Category = "pages", Name = "Load Balancer P.4", Content = page4, ContentVector = vectorPage4, ContentVector2 = vectorPage4, ContentVector3 = vectorPage4, ContentVector4 = vectorPage4 },
            
            // new Book() { Id = "page1_1", Category = "paragraph", Name = "Load Balancer P.1.1", Content = page1_1, ContentVector = GetEmbeddings(page1_1) },
            // new Book() { Id = "page1_2", Category = "paragraph", Name = "Load Balancer P.1.2", Content = page1_2, ContentVector = GetEmbeddings(page1_2) },
            // new Book() { Id = "page2_1", Category = "paragraph", Name = "Load Balancer P.2.1", Content = page2_1, ContentVector = GetEmbeddings(page2_1) },
            // new Book() { Id = "page2_2", Category = "paragraph", Name = "Load Balancer P.2.2", Content = page2_2, ContentVector = GetEmbeddings(page2_2) },
            // new Book() { Id = "page2_3", Category = "paragraph", Name = "Load Balancer P.2.3", Content = page2_3, ContentVector = GetEmbeddings(page2_3) },
            // new Book() { Id = "page2_4", Category = "paragraph", Name = "Load Balancer P.2.4", Content = page2_4, ContentVector = GetEmbeddings(page2_4) },
            // new Book() { Id = "page3_1", Category = "paragraph", Name = "Load Balancer P.3.1", Content = page3_1, ContentVector = GetEmbeddings(page3_1) },
            // new Book() { Id = "page3_2", Category = "paragraph", Name = "Load Balancer P.3.2", Content = page3_2, ContentVector = GetEmbeddings(page3_2) },
            // new Book() { Id = "page4_1", Category = "paragraph", Name = "Load Balancer P.4.1", Content = page4_1, ContentVector = GetEmbeddings(page4_1) },
            // new Book() { Id = "page4_2", Category = "paragraph", Name = "Load Balancer P.4.2", Content = page4_2, ContentVector = GetEmbeddings(page4_2) },
            // new Book() { Id = "page4_3", Category = "paragraph", Name = "Load Balancer P.4.3", Content = page4_3, ContentVector = GetEmbeddings(page4_3) },
            // new Book() { Id = "page4_4", Category = "paragraph", Name = "Load Balancer P.4.4", Content = page4_4, ContentVector = GetEmbeddings(page4_4) }
        };
    }
    #endregion
    
}
