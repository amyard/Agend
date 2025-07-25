// See https://aka.ms/new-console-template for more information

using System.ClientModel;
using System.ComponentModel;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Text.Json;
using Azure.AI.OpenAI;
using FactManagement.Models;
using Helper.Models;
using OpenAI.Chat;
using OpenAI.Embeddings;

Console.WriteLine("Hello, World!");

string jsonString = File.ReadAllText("jsondata/secrets.json");
AzureOpenAISettings azureOpenAiSettings = JsonSerializer.Deserialize<AzureOpenAISettings>(jsonString);

// get clients
AzureOpenAIClient openAIClient = new AzureOpenAIClient(new Uri(azureOpenAiSettings.ChatCompletion.Endpoint), new ApiKeyCredential(azureOpenAiSettings.ChatCompletion.ApiKey));
ChatClient chatClient = openAIClient.GetChatClient(azureOpenAiSettings.ChatCompletion.Deployment);


// get data
List<PdfPageContent> doc4 = Helper.AsposePdfExtractor.ExtractPdfContentByParagraphDS_v1("text/report.pdf");
StringBuilder sb = new StringBuilder();
foreach (PdfPageContent doc in doc4)
{
    sb.AppendLine($"<{doc.PageNumber}>");
    sb.AppendLine(string.Join("/n", doc.Content));
    sb.AppendLine($"</{doc.PageNumber}>");
    sb.AppendLine();
}
string text = sb.ToString();
var lol = 1;


// extract Facts
List<LegalFact> facts =  await ExtractFactsFromDocumentAsync(text, chatClient).ConfigureAwait(false);

// store as json
string projectRoot = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", ".."));
string outputPath = Path.Combine(projectRoot, "jsondata", "LegalFacts.json");
string jsonData = JsonSerializer.Serialize(facts, new JsonSerializerOptions { WriteIndented = true });
File.WriteAllText(outputPath, jsonData);

// similarity between facts
// await CheckSimilarity(openAIClient, azureOpenAiSettings).ConfigureAwait(false);


async Task<List<LegalFact>> ExtractFactsFromDocumentAsync(string text, ChatClient chatClient)
{
    int maxOutputTokens = 4000;
    string systemMessage = File.ReadAllText(Path.Combine("prompt", "system-message.txt"));
    string rulesMessage = File.ReadAllText(Path.Combine("prompt", "rules.txt"));
    string rules = rulesMessage
        .Replace("{{nameof(LegalFact.FactStatement)}}", nameof(LegalFact.FactStatement))
        .Replace("{{nameof(LegalFact.FactText)}}", nameof(LegalFact.FactText))
        .Replace("{{nameof(LegalFact.Dates)}}", nameof(LegalFact.Dates))
        .Replace("{{nameof(LegalFact.Page)}}", nameof(LegalFact.Page))
        .Replace("{{nameof(LegalFact.InvolvedParties)}}", nameof(LegalFact.InvolvedParties))
        .Replace("{{nameof(LegalFact.LegalIssue)}}", nameof(LegalFact.LegalIssue))
        .Replace("{{nameof(LegalFact.ImportanceLevel)}}", nameof(LegalFact.ImportanceLevel))
        .Replace("{{maxOutputTokens}}", maxOutputTokens.ToString());
    
    
    // string systemMessage = """
    //     You are a legal analyst. Analyze the following document and extract all factual statements that could be relevant in a court of law.
    //     
    //     Always respond with a valid JSON object that conforms to the schema provided. Do not include any explanatory text, formatting instructions, or commentary outside the JSON.
    //     
    //     Each fact must be:
    //     - Directly stated in the document (no assumptions or interpretations)
    //     - Relevant to legal proceedings
    //     - Categorized by type and importance
    //     """;
    
    // Construct the prompt with legal-specific instructions
//     string rules = $$"""
//                      Please! Do not wrap the response with ```json ``` or any other formatting guidance.
//                      
//                      Extract and list the key factual statements from the following court document. Focus only on objective facts, such as:
//                      - Names of the parties involved
//                      - Important dates and timelines
//                      - Legal claims and allegations
//                      - Factual background of the dispute
//                      - Key findings or facts recognized by the court
//                      - Actions taken by either party
//                      - Court decisions (fact-based, not legal reasoning)
//                      Do not include legal opinions, arguments, or citations. Present the facts as bullet points, each representing a discrete factual event or condition. Do not include legal arguments, opinions, or conclusions.
//                      
//                      For each fact, include:
//                      - {{nameof(LegalFact.FactStatement)}}
//                      - {{nameof(LegalFact.FactText)}}
//                      - {{nameof(LegalFact.Dates)}}
//                      - {{nameof(LegalFact.Page)}}
//                      - {{nameof(LegalFact.InvolvedParties)}}
//                      - {{nameof(LegalFact.LegalIssue)}}
//                      - {{nameof(LegalFact.ImportanceLevel)}}
//                      
//                      Rules for {{nameof(LegalFact.FactStatement)}}
//                      - A clear, concise description of both the fact itself and how it relates to the case
//                      
//                      Rule for {{nameof(LegalFact.FactText)}}
//                      - Provide text from provided document. Do not add any hallucinations or explanations. Clear text described {{nameof(LegalFact.FactStatement)}}
//                      
//                      Rules for {{nameof(LegalFact.Dates)}}
//                      - When the fact occurred (can be a range)
//                      
//                      Rules for {{nameof(LegalFact.Page)}}
//                      - Source location using first page number only, or first-last if spanning multiple pages (e.g., "123" or "123-125")
//                      
//                      Rules for {{nameof(LegalFact.InvolvedParties)}}
//                      - All relevant people, organizations, or entities connected to the fact
//                      
//                      Rules for {{nameof(LegalFact.LegalIssue)}}
//                      - Which legal issue the fact relates to
//                      
//                      Rules for {{nameof(LegalFact.ImportanceLevel)}}
//                      - Subjective rating (e.g., 1–5), where 1 is low and 5 is high
//                      
//                      Focus exclusively on:
//                      ✓ Specific dates/times/locations
//                      ✓ Identifiable people/organizations/places
//                      ✓ Quantifiable amounts/measurements
//                      ✓ Direct statements of events/actions
//                      ✓ Contract terms/agreement details
//                      ✓ Financial transactions/obligations
//                      ✓ Property descriptions/ownership
//                      ✓ Employment/professional relationships
//                      
//                      Exclude:
//                      × Legal arguments or theories
//                      × Subjective opinions
//                      × Conclusions or interpretations
//                      × Information not directly stated in text
//                      
//                      Document pages are marked as <1>content</1>, <2>content</2>, etc.
//                      
//                      Return only the JSON object as a string, without any additional text, formatting, or commentary. 
//                      Do not include any explanations, citations or additional information outside the JSON string.
//                      The output result should be less than {{maxOutputTokens}} tokens. 
//                      If the output exceeds {maxOutputTokens} tokens, prioritize facts with the highest ImportanceLevel and those most directly tied to the central legal issue.
//                      """;
    
#pragma warning disable AOAI001
    var schema = await File.ReadAllTextAsync(Path.Combine("prompt", "schema", "legal-fact.schema.json"));
    
    ChatCompletionOptions chatCompletionsOptions = new()
    {
        Temperature = 1,
        MaxOutputTokenCount = maxOutputTokens,
        ResponseFormat = ChatResponseFormat.CreateJsonSchemaFormat(
            jsonSchemaFormatName: "legal-fact-schema",
            jsonSchema: BinaryData.FromString(schema)
        )
    };
#pragma warning restore AOAI001
    
    ChatCompletion completion = await chatClient.CompleteChatAsync(
    [
        new SystemChatMessage(systemMessage),
        new UserChatMessage("The following information is from PDF document: " + text),
        new UserChatMessage(rules),
    ], options: chatCompletionsOptions).ConfigureAwait(false);

    string botResponse = completion.Content.First().Text?.Replace("```json", "").Replace("```", "") ?? String.Empty;
    LegalFactWrapper result  = JsonSerializer.Deserialize<LegalFactWrapper>(botResponse);
    
    List<LegalFact> facts = result?.Facts ?? [];

    return facts;
}


async Task CheckSimilarity(AzureOpenAIClient openAIClient, AzureOpenAISettings azureOpenAiSettings)
{
    List<string> sentences = new List<string>
    {
        "The quick brown fox jumps over the lazy dog.",
        "A fast fox leaps over a sleeping dog.",
        "I enjoy reading books about science.",
        "Artificial intelligence is transforming industries.",
        "The dog is sleeping peacefully in the sun.",
        "Machine learning models require large datasets.",
        "A lazy dog is lying under a tree.",
        "Science books fascinate me with new discoveries.",
        "Industries are being reshaped by AI advancements.",
        "The fox jumps swiftly over the dog."
    };
    
    EmbeddingClient embeddingClient = openAIClient.GetEmbeddingClient(azureOpenAiSettings.Embedding.Deployment);
    EmbeddingGenerationOptions embeddingOptions = new() { Dimensions = azureOpenAiSettings.Embedding.Dimensions };
    
    OpenAIEmbeddingCollection responseEmbedding = await embeddingClient.GenerateEmbeddingsAsync(sentences, embeddingOptions).ConfigureAwait(false);

    List<float[]> embeddings = [];
    foreach (OpenAIEmbedding openAiEmbedding in responseEmbedding)
    {
        embeddings.Add(openAiEmbedding.ToFloats().ToArray());
    }
    
    // Compare all pairs and store results
    var similarityMatrix = new float[sentences.Count, sentences.Count];
    for (int i = 0; i < sentences.Count; i++)
    {
        for (int j = i; j < sentences.Count; j++)
        {
            float similarity = CosineSimilaritySimd(embeddings[i], embeddings[j]);
            similarityMatrix[i, j] = similarity;
            similarityMatrix[j, i] = similarity; // Symmetric
        }
    }

    // Print top 3 similar pairs per sentence
    for (int i = 0; i < sentences.Count; i++)
    {
        Console.WriteLine($"\nTop matches for: '{sentences[i]}'");
        var rankedMatches = Enumerable.Range(0, sentences.Count)
            .Where(j => j != i)
            .OrderByDescending(j => similarityMatrix[i, j])
            .Take(3);

        foreach (var j in rankedMatches)
        {
            Console.WriteLine($"  - Similarity: {similarityMatrix[i, j]:F2} | '{sentences[j]}'");
        }
    }
}

// SIMD-optimized cosine similarity (for float[] embeddings)
float CosineSimilaritySimd(float[] a, float[] b)
{
    if (a.Length != b.Length)
        throw new ArgumentException("Vectors must be the same length.");

    int vectorSize = Vector<float>.Count;
    int i = 0;
    Vector<float> dotSum = Vector<float>.Zero;
    Vector<float> magASum = Vector<float>.Zero;
    Vector<float> magBSum = Vector<float>.Zero;

    // Process vectorized blocks
    for (; i <= a.Length - vectorSize; i += vectorSize)
    {
        var va = new Vector<float>(a, i);
        var vb = new Vector<float>(b, i);
        dotSum += va * vb;
        magASum += va * va;
        magBSum += vb * vb;
    }

    // Manually sum the elements of the SIMD vectors
    float dot = 0, magA = 0, magB = 0;
    for (int j = 0; j < vectorSize; j++)
    {
        dot += dotSum[j];
        magA += magASum[j];
        magB += magBSum[j];
    }

    // Sum remaining elements (non-vectorized)
    for (; i < a.Length; i++)
    {
        dot += a[i] * b[i];
        magA += a[i] * a[i];
        magB += b[i] * b[i];
    }

    return dot / (MathF.Sqrt(magA) * MathF.Sqrt(magB));
}
