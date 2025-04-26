using Microsoft.Extensions.AI;

namespace Chat;

public class Summarization
{
    internal static async Task Call(IChatClient chatClient)
    {
        var posts = Directory.GetFiles("posts", "*.txt").ToArray();
        
        foreach (string post in posts)
        {
            Console.WriteLine("AAAAAAAAAAAAAAAAAAAAAAAAA");
            string prompt = 
                $$"""
                  You will receive an input text and desired output format.
                  You need to analyze the text and produce the desired output format.
                  You not allow to change code, text or other references.
                  
                  # Desired response
                  
                  Only provide a RFC8259 compliant JSON response following this format without deviation.
                  
                  {
                      "title": "Title pulled from the front matter section",
                      "summary": "Summarize the article in no more than 100 words"
                  }
                  
                  # Article content:
                  
                  {{File.ReadAllText(post)}}
                  """;

            var chatCompletion = await chatClient.CompleteAsync(prompt);
            
            Console.WriteLine(chatCompletion.Message.Text);
            Console.WriteLine(Environment.NewLine);
        }
    }
}
