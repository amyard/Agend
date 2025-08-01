using System.Text.Json;
using Chronology.Models;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.InMemory;
using Microsoft.SemanticKernel.Data;
using Microsoft.SemanticKernel.Embeddings;
using Kernel = Microsoft.SemanticKernel.Kernel;

var builder = Kernel.CreateBuilder();

string jsonString = File.ReadAllText("Data/secrets.json");
AzureOpenAISettings azureOpenAiSettings = JsonSerializer.Deserialize<AzureOpenAISettings>(jsonString);

#pragma warning disable SKEXP0020
#pragma warning disable SKEXP0010
#pragma warning disable SKEXP0001

builder.AddAzureOpenAIChatCompletion(azureOpenAiSettings.ChatCompletion.Deployment, azureOpenAiSettings.ChatCompletion.Endpoint, azureOpenAiSettings.ChatCompletion.ApiKey, modelId: azureOpenAiSettings.ChatCompletion.ModelId);
builder.AddAzureOpenAIEmbeddingGenerator(azureOpenAiSettings.Embedding.Deployment, azureOpenAiSettings.Embedding.Endpoint, azureOpenAiSettings.Embedding.ApiKey, modelId: azureOpenAiSettings.Embedding.ModelId);

    
// vector database store
string collectionName = "myknowledgebase";
string rootPath = "knowledgebase";
var vectorStore = new InMemoryVectorStore();


// builder.AddInMemoryVectorStore()
//     .AddInMemoryVectorStoreRecordCollection<string, KnowledgeBaseSnippet>(collectionName)
//     .AddVectorStoreTextSearch<KnowledgeBaseSnippet>(
//         new TextSearchStringMapper((result) => (result as KnowledgeBaseSnippet)!.Text!),
//         new TextSearchResultMapper((result) => 
//         {
//             // create a mapping from the Vector Store data type to the data type returned by the Text Search,
//             // this search will ultimately be used in a plugin and this TextSearchResult will be retuned to the prompt tenplate
//             // when the the plugin is invoked from the prompt template
//             var castResult = result as KnowledgeBaseSnippet;
//             return new TextSearchResult(value: castResult!.Text!)
//                 { Name = castResult.ReferenceDescription, Link = castResult.ReferenceLink };
//         })
//         );
    
var kernel = builder.Build();

Console.WriteLine("Hello, World!");

// (string doc1, string doc2) = Helper.PdfDocumentHelper.ExtractFullTextFromDocument("text/report.pdf");

// var doc3 = Helper.AsposePdfExtractor.ExtractContentByParagraph("text/report.pdf");
// var doc4 = Helper.AsposePdfExtractor.ExtractPdfContentByParagraphDS_v1("text/report.pdf");
// var doc4 = Helper.AsposePdfExtractor.ExtractPdfContentByParagraphDS_v1("text/azure_load_balancer.pdf");
// var doc5 = Helper.AsposePdfExtractor.ExtractPdfContentByParagraph_V2("text/report.pdf");
// var doc6 = Helper.AsposePdfExtractor.ExtractPdfContentByParagraph_V3("text/report.pdf");

// var doc5 = Helper.AsposePdfExtractor.ExtractPdfContentByParagraphAdvanced("text/report.pdf");

var lol = 1;
