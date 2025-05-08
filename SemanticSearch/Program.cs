using Helper;
using Microsoft.Extensions.AI;
using SemanticSearch;


# region Simple semantic search with in memory db
// await SimpleEmbedding.Run();
# endregion


# region Qdtant db - simple result
// await QdrantSimple.Run();
// await QdrantSentences.Run();
await QdrantProd.Run();
# endregion


// (string, string) lol = PdfDocumentHelper.ExtractFullTextFromDocument("text/report.pdf");

Console.WriteLine("Hello, World!");
