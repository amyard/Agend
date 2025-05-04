using Helper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ChromaDB.Client;
using Microsoft.Extensions.AI;

# region Simple semantic search with in memory db
// await SimpleEmbedding.Run();
# endregion


# region Chroma db - simple result 
// var options = new ChromaConfigurationOptions("http://localhost:8000/api/v1");
// var options = new ChromaConfigurationOptions("http://localhost:8000");
var options = new ChromaConfigurationOptions(uri: new Uri("http://localhost:8000/api/v2") );
// var options = new ChromaConfigurationOptions("http://172.18.0.2:8000/api/v2");
using var httpClient = new HttpClient();
var client = new ChromaClient(options, httpClient);

// var lol = await client.Heartbeat();
// Console.WriteLine(lol.NanosecondHeartbeat);


var collection = await client.GetOrCreateCollection("delme");
var collectionClient = new ChromaCollectionClient(collection, options, httpClient);

IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator =
    new OllamaEmbeddingGenerator(new Uri("http://localhost:11434"), modelId: "all-minilm:22m");

foreach (var item in new[] {"shy", "jiri", "chimpanzee"})
{
    ReadOnlyMemory<float> vector = await embeddingGenerator.GenerateEmbeddingVectorAsync(item);
    await collectionClient.Add([item], [vector], [new(){{"name", item}}]);
}




# endregion





// (string, string) lol = PdfDocumentHelper.ExtractFullTextFromDocument("text/report.pdf");

Console.WriteLine("Hello, World!");
