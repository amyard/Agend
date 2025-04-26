using Microsoft.Extensions.AI;

namespace Chat;

public static class ChatWithHistory
{
    internal static async Task Call(IChatClient chatClient)
    {
        var chatHistory = new List<ChatMessage>();
        while (true)
        {
            Console.WriteLine("Your prompt:");
            var userPrompt = Console.ReadLine();
            chatHistory.Add(new ChatMessage(ChatRole.User, userPrompt));
            
            // Stream the AI response and add to chat history
            Console.WriteLine("AI Response");
            var chatResponse = "";
            
            await foreach (var item in chatClient.GetStreamingResponseAsync(chatHistory))
            {
                Console.Write(item.Text);
                chatResponse += item.Text;
            }
            
            chatHistory.Add(new ChatMessage(ChatRole.Assistant, chatResponse));
            Console.WriteLine();
        }
    }
}
