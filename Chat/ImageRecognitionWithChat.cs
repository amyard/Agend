using Microsoft.Extensions.AI;

namespace Chat;

public class ImageRecognitionWithChat
{
    private static readonly string _projectRoot = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;

    
    internal static async Task Call(IChatClient chatClient)
    {
        string imageFolder = Path.Combine(_projectRoot, "images");
        string imagePath = Path.Combine(imageFolder, "image2.jpg");
        
        var chatHistory = new List<ChatMessage>();
        
        // step 1. load image and retrieve result
        ChatMessage message = new ChatMessage(ChatRole.User, "What is on the image? Response should be 100 words maximum.");
        message.Contents.Add(new DataContent(File.ReadAllBytes(imagePath), "image/jpg"));
        chatHistory.Add(message);
        
        var chatCompletion = await chatClient.GetResponseAsync([message]);
        Console.WriteLine(chatCompletion.Text);
        chatHistory.Add(new ChatMessage(ChatRole.Assistant, chatCompletion.Text));
        
        // step 2. chatting
        chatHistory.Add(new ChatMessage(ChatRole.System, "You are an AI assistant. Your responses should contain only 50 words."));
        
        while (true)
        {
            Console.WriteLine("Your prompt:");
            var userPrompt = Console.ReadLine();

            if (string.Equals(userPrompt, @"\q", StringComparison.OrdinalIgnoreCase))
                break;
            
            ChatMessage userPromptChatMessage = new ChatMessage(ChatRole.User, userPrompt);
            userPromptChatMessage.Contents.Add(new DataContent(File.ReadAllBytes(imagePath), "image/jpg"));
            
            chatHistory.Add(userPromptChatMessage);
            
            // Stream the AI response and add to chat history
            Console.WriteLine("AI Response");
            var chatResponse = "";
            
            await foreach (var item in chatClient.GetStreamingResponseAsync(chatHistory))
            {
                Console.Write(item.Text);
                chatResponse += item.Text;
            }
            
            ChatMessage chatResponseMessage = new ChatMessage(ChatRole.User, chatResponse);
            
            chatHistory.Add(chatResponseMessage);
            Console.WriteLine();
        }
    }
}
