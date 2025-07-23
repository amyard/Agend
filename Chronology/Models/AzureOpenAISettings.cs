namespace Chronology.Models;

public class AzureOpenAISettings
{
    public required ChatCompletionSettings ChatCompletion { get; init; }

    public required EmbeddingSettings Embedding { get; init; }
}

public class ServiceSettings
{
    public required string Endpoint { get; init; }

    public required string Deployment { get; init; }

    public required string ModelId { get; init; }

    public required string ApiKey { get; init; }
}

public class ChatCompletionSettings : ServiceSettings
{
    public int MaxTokenSize { get; set; }
}

public class EmbeddingSettings : ServiceSettings
{
    public int? Dimensions { get; set; }
}
