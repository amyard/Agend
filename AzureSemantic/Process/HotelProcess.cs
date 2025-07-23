namespace AzureSemantic.Process;

using Azure.Search.Documents;
using Azure.Search.Documents.Indexes;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using AzureSemantic.Helpers;
using AzureSemantic.Models;

public static class HotelProcess
{
    private static string _indexName = "hotel-index-test";
    
    internal static async Task Process()
    {
        SearchIndexClient searchIndexClient = SemanticSearchHelper.GetSearchIndexClient();
        SearchClient searchClient = SemanticSearchHelper.GetSearchClient(_indexName);

        bool callOnce = true;

        if (!callOnce)
        {
            // prepare index model
            SearchIndex hotelSearchIndex = SemanticSearchHelper.PrepareHotelSearchIndex(_indexName);

            // Create index
            await searchIndexClient.CreateIndexAsync(hotelSearchIndex).ConfigureAwait(false);

            // upload data
            Hotel[] hotelDocuments = SemanticSearchHelper.GetHotelDocuments();
            await searchClient.IndexDocumentsAsync(IndexDocumentsBatch.Upload(hotelDocuments)).ConfigureAwait(false);
    
        }

        // search - Using VectorizableTextQuery - convert search term into embedding array
        await HotelSearchHelper.SingleVectorSearchEmbedding(searchClient, "Top hotels in town");
        await HotelSearchHelper.SingleHybridSearchEmbedding(searchClient, "Top hotels in town");

        // using UsingVectorizableTextQuery - do not transfer search to embedding
        await HotelSearchHelper.SingleVectorSearchNotEmbedding(searchClient, "Top hotels in town");
    }
}
