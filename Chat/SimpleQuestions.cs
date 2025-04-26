using Microsoft.Extensions.AI;

namespace Chat;

public static class SimpleQuestions
{
    internal static async Task Call(IChatClient chatClient)
    {
        var chatCompletion = await chatClient.CompleteAsync("What is .Net? Reply in 50 words max.");
        Console.WriteLine(chatCompletion.Message.Text);
    }
}
