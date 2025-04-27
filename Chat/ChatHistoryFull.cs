using System.Text.Json;
using Microsoft.Extensions.AI;

namespace Chat;

internal static class ChatHistoryFull
{
    private static readonly string _projectRoot = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName;
    private static readonly string _historyFolderName = "history";
    
    public static async Task Call(IChatClient chatClient, int historyId, string userId = "111-222-3333")
    {
        EnsureHistoryDirectoryExists(historyId);
        List<ChatMessage> chatHistory = LoadChatHistory(historyId);
        
        if (chatHistory.Count == 0)
            chatHistory.Insert(0, new ChatMessage(ChatRole.System, "You are an AI assistant. Your responses should contain only 50 words."));
        
        while (true)
        {
            Console.WriteLine("Your prompt:");
            var userPrompt = Console.ReadLine();

            if (string.Equals(userPrompt, @"\q", StringComparison.OrdinalIgnoreCase))
                break;
            
            ChatMessage userPromptChatMessage = new ChatMessage(ChatRole.User, userPrompt)
            {
                AuthorName = userId,
                AdditionalProperties = new AdditionalPropertiesDictionary
                {
                    { "Date", DateTime.UtcNow.ToString("yyyy-MM-dd") }
                }
            };
            
            chatHistory.Add(userPromptChatMessage);
            
            // Stream the AI response and add to chat history
            Console.WriteLine("AI Response");
            var chatResponse = "";
            
            await foreach (var item in chatClient.GetStreamingResponseAsync(chatHistory))
            {
                Console.Write(item.Text);
                chatResponse += item.Text;
            }
            
            ChatMessage chatResponseMessage = new ChatMessage(ChatRole.User, chatResponse)
            {
                AuthorName = userId,
                AdditionalProperties = new AdditionalPropertiesDictionary
                {
                    { "Date", DateTime.UtcNow.ToString("yyyy-MM-dd") }
                }
            };
            
            chatHistory.Add(chatResponseMessage);
            Console.WriteLine();
        }

        StoreHistory(chatHistory, historyId);
        
        Console.WriteLine("FINISH !!!");
    }

    private static void StoreHistory(List<ChatMessage> chatHistory, int historyId)
    {
        string json = JsonSerializer.Serialize(chatHistory);
        string folderPath = Path.Combine(_projectRoot, _historyFolderName, historyId.ToString());
        string fileName = $"{DateTime.UtcNow:yyyy-MM-dd}.json";
        string fullPath = Path.Combine(folderPath, fileName);
        
        try
        {
            File.WriteAllText(fullPath, json);
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Error writing history file: {ex.Message}");
        }
    }

    private static void EnsureHistoryDirectoryExists(int historyId)
    {
        var path = Path.Combine(_projectRoot, _historyFolderName, historyId.ToString());

        if (Directory.Exists(path)) return;
        
        try
        {
            Directory.CreateDirectory(path);
        }
        catch (IOException ex)
        {
            Console.WriteLine($"Error creating history directory: {ex.Message}");
        }
    }

    private static List<ChatMessage> LoadChatHistory(int historyId)
    {
        var chatHistory = new List<ChatMessage>();
        var historyPath = Path.Combine(_projectRoot, _historyFolderName, historyId.ToString());
        
        foreach (var filePath in Directory.GetFiles(historyPath, "*.json"))
        {
            try
            {
                var json = File.ReadAllText(filePath);

                if (string.IsNullOrWhiteSpace(json))
                    continue;

                var data = JsonSerializer.Deserialize<List<ChatMessage>>(json);
                if (data != null)
                    chatHistory.AddRange(data);
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Failed to load {filePath}: {ex.Message}");
            }
            catch (IOException ex)
            {
                Console.WriteLine($"IO Error reading {filePath}: {ex.Message}");
            }
        }
        
        return chatHistory;
    }
    
    
    
}
