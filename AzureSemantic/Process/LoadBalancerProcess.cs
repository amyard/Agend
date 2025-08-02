namespace AzureSemantic.Process;

using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using AzureSemantic.Helpers;
using AzureSemantic.Models;

public static class LoadBalancerProcess
{
    private static string _indexName = "load-balancer-index-test";
    
    internal static async Task ProcessCreateIndex()
    {
        SearchIndexClient searchIndexClient = SemanticSearchHelper.GetSearchIndexClient();
        SearchClient searchClient = SemanticSearchHelper.GetSearchClient(_indexName);

        // prepare index model
        SearchIndex loadBalancerSearchIndex = SemanticSearchHelper.PrepareLoadBalancerSearchIndex(_indexName);

        // Create index
        await searchIndexClient.CreateIndexAsync(loadBalancerSearchIndex).ConfigureAwait(false);

        // upload data
        Book[] loadBalancerDocuments = SemanticSearchHelper.GetLoadBalancerDocuments();
        await searchClient.IndexDocumentsAsync(IndexDocumentsBatch.Upload(loadBalancerDocuments)).ConfigureAwait(false);
    }
    
    internal static async Task ProcessSearch()
    {
        SearchClient searchClient = SemanticSearchHelper.GetSearchClient(_indexName);

        // search - Using VectorizableTextQuery - convert search term into embedding array
        // await LoadBalancerSearchHelper.SingleVectorSearchEmbedding(searchClient, "load balancer", "Category eq 'doc'");
        // await LoadBalancerSearchHelper.SingleVectorSearchEmbedding(searchClient, "load balancer", "Category eq 'pages'");
        // await LoadBalancerSearchHelper.SingleVectorSearchEmbedding(searchClient, "load balancer", "Category eq 'paragraph'");

        string query = "load balancer";
        
        // await LoadBalancerSearchHelper.SingleVectorSearchEmbedding(searchClient, query, "Category eq 'doc'");
        // await LoadBalancerSearchHelper.SingleVectorSearchEmbedding(searchClient, query, "Category eq 'pages'");
        // await LoadBalancerSearchHelper.SingleVectorSearchEmbedding(searchClient, query, "Category eq 'paragraph'");
        
        // await LoadBalancerSearchHelper.SingleVectorSearchNotEmbedding(searchClient, query, "Category eq 'doc'");
        // await LoadBalancerSearchHelper.SingleVectorSearchNotEmbedding(searchClient, query, "Category eq 'pages'");
        // await LoadBalancerSearchHelper.SingleVectorSearchNotEmbedding(searchClient, query, "Category eq 'paragraph'");
        
        // await LoadBalancerSearchHelper.SingleVectorSearchEmbedding(searchClient, query, "Category eq 'pages'");
        // await LoadBalancerSearchHelper.SingleVectorSearchNotEmbedding(searchClient, query, "Category eq 'pages'");
        
        // await HotelSearchHelper.SingleHybridSearchEmbedding(searchClient, "Top hotels in town");
        //
        // // using UsingVectorizableTextQuery - do not transfer search to embedding
        // await HotelSearchHelper.SingleVectorSearchNotEmbedding(searchClient, "Top hotels in town");
        
        ReadOnlyMemory<float> embeddingQuery = SemanticSearchHelper.GetEmbeddings(query);
        
        // await LoadBalancerSearchHelper.FastVectorSearch(searchClient, query, embeddingQuery, "Category eq 'pages'");
        // await LoadBalancerSearchHelper.BalancedVectorSearch(searchClient, query, embeddingQuery, filter: "Category eq 'pages'");
        // await LoadBalancerSearchHelper.ExactVectorSearch(searchClient, query, embeddingQuery, "Category eq 'pages'");
        // await LoadBalancerSearchHelper.HybridSearch(searchClient, query, embeddingQuery, "Category eq 'pages'");
        
        // await LoadBalancerSearchHelper.BalancedVectorSearch(searchClient, query, embeddingQuery, nameof(Book.ContentVector4), "Category eq 'pages'");
        
        // await LoadBalancerSearchHelper.BalancedVectorSemanticSearch(searchClient, query, embeddingQuery, nameof(Book.ContentVector4), "Category eq 'pages'");
        // await LoadBalancerSearchHelper.BalancedVectorSemanticSearchWithPagination(searchClient, query, embeddingQuery, nameof(Book.ContentVector4), "Category eq 'pages'");
        
        // "Category eq 'pages'"
        // await LoadBalancerSearchHelper.EnhancedSemanticSearch(searchClient, query, embeddingQuery, nameof(Book.ContentVector4), "Category eq 'pages'");
        
        // "Category eq 'pages' and Id eq 'page1'"
        await LoadBalancerSearchHelper.EnhancedSemanticSearch(searchClient, query, embeddingQuery, nameof(Book.ContentVector4), "Category eq 'pages' and Id eq 'page2'");
    }
}
