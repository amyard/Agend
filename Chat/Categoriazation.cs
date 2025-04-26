using Microsoft.Extensions.AI;

namespace Chat;

public class Categoriazation
{
    internal static async Task Call(IChatClient chatClient)
    {
        Console.WriteLine("Categoriazation");
        
        var posts = Directory.GetFiles("posts", "*.txt").ToArray();
        
        ChatOptions options = new ChatOptions()
        {
            Temperature = 1,
            MaxOutputTokens = 300,
            // ChatThreadId = this.ChatThreadId,
            // TopP = this.TopP,
            // TopK = this.TopK,
            // FrequencyPenalty = this.FrequencyPenalty,
            // PresencePenalty = this.PresencePenalty,
            // Seed = this.Seed,
            // ResponseFormat = this.ResponseFormat,
            // ModelId = this.ModelId,
            // ToolMode = this.ToolMode,
            // AdditionalProperties = this.AdditionalProperties?.Clone()
        };

        string systemString = $$"""
                                 You will receive an input text and desired output format.
                                 You need to analyze the text and produce the desired output format.
                                 You not allow to change code, text or other references.
                                 You are an expert at parsing text content into a structured JSON document.
                                 For any given input, always respond with a JSON document that accurately represents the content and complies with the provided JSON Schema.
                                 Do not include any other text or values in your response.
                                 """;
        
        string responsePromptWithRules = $$"""
                                 # Desired response
                                 
                                 Only provide a RFC8259 compliant JSON response following this format without deviation.
                                 
                                 {
                                     "Title": "Title pulled from the front matter section",
                                     "Tags": "Array of tags based on analyzing the article content. Tags should be lowercase."
                                 }
                                 """;
        
        string userText = $$"""
                            # Article content:
                            
                            {0}
                            """;
        
        ChatMessage systemMessage = new ChatMessage(ChatRole.System, systemString);
        ChatMessage userMessageResponsePrompt = new ChatMessage(ChatRole.User, responsePromptWithRules);
        
        foreach (string post in posts)
        {
            ChatMessage userMessageText = new ChatMessage(ChatRole.User, string.Format(userText, File.ReadAllText(post)));

            var chatCompletion = await chatClient.GetResponseAsync<PostCategory>(
                [systemMessage, userMessageResponsePrompt, userMessageText],
                options: options);
            
            string cleanedResponse = chatCompletion.Text?.Replace("```json", "").Replace("```", "") ?? String.Empty;
            
            Console.WriteLine(cleanedResponse);
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
