using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace SemanticKernelTests;

public static class ChatSimple
{
    internal static async Task Run()
    {
        IKernelBuilder kernelBuilder = Kernel.CreateBuilder();
        
#pragma warning disable SKEXP0070
        kernelBuilder.AddOllamaChatCompletion(
            modelId: "NAME_OF_MODEL",           // E.g. "phi3" if phi3 was downloaded as described above.
            endpoint: new Uri("YOUR_ENDPOINT"), // E.g. "http://localhost:11434" if Ollama has been started in docker as described above.
            serviceId: "SERVICE_ID"             // Optional; for targeting specific services within Semantic Kernel
        );
#pragma warning restore SKEXP0070
        
        kernelBuilder.Services.AddTransient((serviceProvider)=> new Kernel(serviceProvider));
        
        Kernel kernel = kernelBuilder.Build();
        var chatCompletionService = kernel.GetRequiredService<IChatCompletionService>();
        
        var history = new ChatHistory();
        history.AddSystemMessage("You are a helpful assistant.");

        while (true)
        {
            Console.Write("You: ");
            var userMessage = Console.ReadLine();

            if (string.IsNullOrWhiteSpace(userMessage))
            {
                break;
            }

            history.AddUserMessage(userMessage);

            var response = await chatCompletionService.GetChatMessageContentAsync(history);

            Console.WriteLine($"Bot: {response.Content}");

            history.AddMessage(response.Role, response.Content ?? string.Empty);
        }
    }
}
