using Chronology.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.Connectors.InMemory;
using Microsoft.SemanticKernel.Data;

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddJsonFile("appsettings.local.json", optional: true, reloadOnChange: true);

var aiSettings = builder.Configuration.GetSection("AzureOpenAI").Get<AzureOpenAISettings>()!;

#pragma warning disable SKEXP0010
builder.Services
    .AddAzureOpenAIEmbeddingGenerator(aiSettings.Embedding.Deployment, aiSettings.Embedding.Endpoint, aiSettings.Embedding.ApiKey, modelId: aiSettings.Embedding.ModelId, dimensions: aiSettings.Embedding.Dimensions)
    .AddAzureOpenAIChatCompletion(aiSettings.ChatCompletion.Deployment, aiSettings.ChatCompletion.Endpoint, aiSettings.ChatCompletion.ApiKey, modelId: aiSettings.ChatCompletion.ModelId);
#pragma warning restore SKEXP0010

// vector database store
string collectionName = "myknowledgebase";
string rootPath = "knowledgebase";
var vectorStore = new InMemoryVectorStore();

builder.Services
    .AddInMemoryVectorStore()
    .AddInMemoryVectorStoreRecordCollection<string, KnowledgeBaseSnippet>(collectionName)
    .AddVectorStoreTextSearch<KnowledgeBaseSnippet>(
        new TextSearchStringMapper((result) => (result as KnowledgeBaseSnippet)!.Text!),
        new TextSearchResultMapper((result) => 
        {
            // create a mapping from the Vector Store data type to the data type returned by the Text Search,
            // this search will ultimately be used in a plugin and this TextSearchResult will be retuned to the prompt tenplate
            // when the the plugin is invoked from the prompt template
            var castResult = result as KnowledgeBaseSnippet;
            return new TextSearchResult(value: castResult!.Text!)
                { Name = castResult.ReferenceDescription, Link = castResult.ReferenceLink };
        })
        );
    
var app = builder.Build();

Console.WriteLine("Hello, World!");

(string doc1, string doc2) = Helper.PdfDocumentHelper.ExtractFullTextFromDocument("text/report.pdf");

// var doc3 = Helper.AsposePdfExtractor.ExtractContentByParagraph("text/report.pdf");
var doc4 = Helper.AsposePdfExtractor.ExtractPdfContentByParagraphDS_v1("text/report.pdf");
// var doc5 = Helper.AsposePdfExtractor.ExtractPdfContentByParagraph_V2("text/report.pdf");
// var doc6 = Helper.AsposePdfExtractor.ExtractPdfContentByParagraph_V3("text/report.pdf");

// var doc5 = Helper.AsposePdfExtractor.ExtractPdfContentByParagraphAdvanced("text/report.pdf");

var lol = 1;
