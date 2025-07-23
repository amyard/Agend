namespace AzureSemantic.Models;

public class Book
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Category { get; set; }
    public string Content { get; set; }
    public ReadOnlyMemory<float> ContentVector { get; set; }
    public ReadOnlyMemory<float> ContentVector2 { get; set; }
    public ReadOnlyMemory<float> ContentVector3 { get; set; }
    public ReadOnlyMemory<float> ContentVector4 { get; set; }
}
