namespace EmbeddingPlayground.Llm.Models;

public sealed class GeminiResponse
{
    public List<Candidate>? candidates { get; set; }
}

public sealed class Candidate
{
    public Content? content { get; set; }
}

public sealed class Content
{
    public List<Part>? parts { get; set; }
}

public sealed class Part
{
    public string? text { get; set; }
}