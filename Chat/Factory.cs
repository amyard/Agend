using Microsoft.Extensions.AI;

namespace Chat;

public interface IChatClientFactory
{
    OllamaChatClient GetClient(string name);
}

public class ChatClientFactory : IChatClientFactory
{
    private readonly Dictionary<string, OllamaChatClient> _clients;

    public ChatClientFactory()
    {
        _clients = new Dictionary<string, OllamaChatClient>
        {
            { "llama", new OllamaChatClient(new Uri("http://localhost:11434"), "llama3.2") },
            { "llava", new OllamaChatClient(new Uri("http://localhost:11434"), "llava") }
        };
    }

    public OllamaChatClient GetClient(string name) => _clients[name];
}
