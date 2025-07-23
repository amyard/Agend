namespace AzureSemantic.Models;

public class Settings
{
    public required string SearchEndpoint { get; set; }
    public required string SearchApiKey { get; set; }
    public required string OpenAiEndpoint { get; set; }
    public required string OpenAiKey { get; set; }
}
