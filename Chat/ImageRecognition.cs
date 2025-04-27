using Microsoft.Extensions.AI;

namespace Chat;

public class ImageRecognition
{
    private static readonly string _projectRoot = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;

    
    internal static async Task Call(IChatClient chatClient)
    {
        string imageFolder = Path.Combine(_projectRoot, "images");
        string imagePath = Path.Combine(imageFolder, "image1.png");
        
        ChatMessage message = new ChatMessage(ChatRole.User, "What is on the image? Response should be 100 words maximum.");
        message.Contents.Add(new DataContent(File.ReadAllBytes(imagePath), "image/png"));
        
        var chatCompletion = await chatClient.GetResponseAsync([message]);
        Console.WriteLine(chatCompletion.Text);
    }
}
