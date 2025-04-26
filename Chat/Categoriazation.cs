using Microsoft.Extensions.AI;

namespace Chat;

public class Categoriazation
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
                      "Title": "Title pulled from the front matter section",
                      "Tags": "Array of tags based on analyzing the article content. Tags should be lowercase."
                  }

                  # Article content:

                  {{File.ReadAllText(post)}}
                  """;

            var chatCompletion = await chatClient.CompleteAsync<PostCategory>(prompt);
            
            Console.WriteLine(chatCompletion.Message.Text);
            Console.WriteLine($"{chatCompletion.Result.Title} => {string.Join(',', chatCompletion.Result.Tags)}");
            Console.WriteLine(Environment.NewLine);
        }
    }
}

class PostCategory
{
    public string Title { get; set; } = string.Empty;
    public string[] Tags { get; set; } = [];
}
