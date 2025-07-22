namespace Chronology.Models;

public class AzureOpenAISettings
{
    public string AZURE_OPENAI_ENDPOINT { get; set; } = string.Empty;
    public string AZURE_OPENAI_KEY { get; set; } = string.Empty;
    public int AZURE_OPENAI_MAX_MODEL_TOKEN_SIZE { get; set; } = 0;
    public string AZURE_OPENAI_TOKEN_ENCODING_NAME { get; set; } = string.Empty;
    public string AZURE_OPENAI_DEPLOYMENT_NAME { get; set; } = string.Empty;
    public string AZURE_OPENAI_DEPLOYMENT_EMBEDDING_NAME { get; set; } = string.Empty;
}
