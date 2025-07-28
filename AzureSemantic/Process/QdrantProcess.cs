using AzureSemantic.Helpers;
using OpenAI.Embeddings;
using Qdrant.Client;
using Qdrant.Client.Grpc;
using static Qdrant.Client.Grpc.Conditions;

namespace AzureSemantic.Process;

public class QdrantProcess
{
    private const string _collectionName = "azure_semantic_collection";
    private const int _dimension = 1032;
    private const string _allPagesCategory = "allpages";
    private const string _pagesCategory = "pages";
    private const string _paragraphCategory = "paragraph";
    
    
    internal static async Task ProcessSearch()
    {
        EmbeddingClient embeddingClient = SemanticSearchHelper.GetEmbeddingClient();
        
        QdrantClient client = new QdrantClient("localhost", 6334);
        await CreateCollection(client).ConfigureAwait(false);
        _ = await PopulateWithDummyData(client).ConfigureAwait(false);
        
        float[] queryVector = SemanticSearchHelper.GetEmbeddings(embeddingClient, "Lames ");
        IReadOnlyList<ScoredPoint> result = await RunQueryWithFilter(client, queryVector).ConfigureAwait(false);
        DisplayResult(result);
        
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
                    Size = _dimension, Distance = Distance.Euclid
                });
        }
    }
    
    private static async Task<UpdateResult> PopulateWithDummyData(QdrantClient client)
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
        
        EmbeddingClient embeddingClient = SemanticSearchHelper.GetEmbeddingClient();
        
        float[] page1_1Vector = SemanticSearchHelper.GetEmbeddings(embeddingClient, page1_1);
        float[] page1_2Vector = SemanticSearchHelper.GetEmbeddings(embeddingClient, page1_2);
        
        float[] page2_1Vector = SemanticSearchHelper.GetEmbeddings(embeddingClient, page2_1);
        float[] page2_2Vector = SemanticSearchHelper.GetEmbeddings(embeddingClient, page2_2);
        float[] page2_3Vector = SemanticSearchHelper.GetEmbeddings(embeddingClient, page2_3);
        float[] page2_4Vector = SemanticSearchHelper.GetEmbeddings(embeddingClient, page2_4);
        
        float[] page3_1Vector = SemanticSearchHelper.GetEmbeddings(embeddingClient, page3_1);
        float[] page3_2Vector = SemanticSearchHelper.GetEmbeddings(embeddingClient, page3_2);
        
        float[] page4_1Vector = SemanticSearchHelper.GetEmbeddings(embeddingClient, page4_1);
        float[] page4_2Vector = SemanticSearchHelper.GetEmbeddings(embeddingClient, page4_2);
        float[] page4_3Vector = SemanticSearchHelper.GetEmbeddings(embeddingClient, page4_3);
        float[] page4_4Vector = SemanticSearchHelper.GetEmbeddings(embeddingClient, page4_4);
        
        float[] page1Vector = SemanticSearchHelper.GetEmbeddings(embeddingClient, page1);
        float[] page2Vector = SemanticSearchHelper.GetEmbeddings(embeddingClient, page2);
        float[] page3Vector = SemanticSearchHelper.GetEmbeddings(embeddingClient, page3);
        float[] page4Vector = SemanticSearchHelper.GetEmbeddings(embeddingClient, page4);
        
        float[] allPagesVector = SemanticSearchHelper.GetEmbeddings(embeddingClient, allPages);
            
            
        UpdateResult operationInfo = await client.UpsertAsync(collectionName: _collectionName, points: new List<PointStruct>
        {
            new() { Id = 1, Vectors = allPagesVector, Payload = { ["category"] = _allPagesCategory, ["name"] = "All Pages" }},
            new() { Id = 2, Vectors = page1Vector, Payload = { ["category"] = _pagesCategory, ["name"] = "Page 1" }},
            new() { Id = 3, Vectors = page2Vector, Payload = { ["category"] = _pagesCategory, ["name"] = "Page 2" }},
            new() { Id = 4, Vectors = page3Vector, Payload = { ["category"] = _pagesCategory, ["name"] = "Page 3" }},
            new() { Id = 5, Vectors = page4Vector, Payload = { ["category"] = _pagesCategory, ["name"] = "Page 4" }},
            new() { Id = 6, Vectors = page1_1Vector, Payload = { ["category"] = _paragraphCategory, ["name"] = "Paragraph 1" }},
            new() { Id = 7, Vectors = page1_2Vector, Payload = { ["category"] = _paragraphCategory, ["name"] = "Paragraph 2" }},
            new() { Id = 8, Vectors = page2_1Vector, Payload = { ["category"] = _paragraphCategory, ["name"] = "Paragraph 3" }},
            new() { Id = 9, Vectors = page2_2Vector, Payload = { ["category"] = _paragraphCategory, ["name"] = "Paragraph 4" }},
            new() { Id = 10, Vectors = page2_3Vector, Payload = { ["category"] = _paragraphCategory, ["name"] = "Paragraph 5" }},
            new() { Id = 11, Vectors = page2_4Vector, Payload = { ["category"] = _paragraphCategory, ["name"] = "Paragraph 6" }},
            new() { Id = 12, Vectors = page3_1Vector, Payload = { ["category"] = _paragraphCategory, ["name"] = "Paragraph 7" }},
            new() { Id = 13, Vectors = page3_2Vector, Payload = { ["category"] = _paragraphCategory, ["name"] = "Paragraph 8" }},
            new() { Id = 14, Vectors = page4_1Vector, Payload = { ["category"] = _paragraphCategory, ["name"] = "Paragraph 9" }},
            new() { Id = 15, Vectors = page4_2Vector, Payload = { ["category"] = _paragraphCategory, ["name"] = "Paragraph 10" }},
            new() { Id = 16, Vectors = page4_3Vector, Payload = { ["category"] = _paragraphCategory, ["name"] = "Paragraph 11" }},
            new() { Id = 17, Vectors = page4_4Vector, Payload = { ["category"] = _paragraphCategory, ["name"] = "Paragraph 12" }},
        });
        
        return operationInfo;
    }
    
    private static async Task<IReadOnlyList<ScoredPoint>> RunQueryWithFilter(QdrantClient client, float[] queryVector)
    {
        IReadOnlyList<ScoredPoint> searchResult = await client.QueryAsync(
            collectionName: _collectionName,
            query: queryVector,
            filter: MatchKeyword("category", _pagesCategory),
            searchParams: new SearchParams { Exact = false, HnswEf = 512 },
            limit: 20,
            payloadSelector: true
        );
        
        return searchResult;
    }
    
    private static void DisplayResult(IReadOnlyList<ScoredPoint> result)
    {
        Console.WriteLine($"Total results: {result.Count}");
        foreach (var point in result.OrderByDescending(x => x.Score))
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
}
