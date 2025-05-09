using Microsoft.KernelMemory;
using Microsoft.KernelMemory.AI.Ollama;


namespace SemanticKernelTests;

public static class SimpleInMemory
{
    internal static async Task Run()
    {
        var config = new OllamaConfig
        {
            Endpoint = "http://localhost:11434",
            TextModel = new OllamaModelConfig("deepseek-r1:1.5b", 131072),
            EmbeddingModel = new OllamaModelConfig("deepseek-r1:1.5b", 2048)
        };

        var memory = new KernelMemoryBuilder()
            .WithOllamaTextGeneration(config)
            .WithOllamaTextEmbeddingGeneration(config)
            .Build<MemoryServerless>();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Processing document ...");
        Console.ResetColor();

        await memory.ImportDocumentAsync("text/report.pdf", documentId: "DOC001");

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("Model is ready for testing.");
        Console.ResetColor();

        while (await memory.IsDocumentReadyAsync("DOC001"))
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Ask your question");
            Console.ResetColor();
    
            var question = Console.ReadLine();
            var answer = await memory.AskAsync(question);
            Console.WriteLine(answer.Result);
    
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine("Sources:");

            foreach (var x in answer.RelevantSources)
            {
                Console.WriteLine($"{x.SourceName} - {x.SourceUrl} - {x.Link}");
            }
            Console.ResetColor();
        }
    }
}
