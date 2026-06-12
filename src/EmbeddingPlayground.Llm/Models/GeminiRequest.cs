namespace EmbeddingPlayground.Llm.Models;

public sealed class GeminiRequest
{
    public List<GeminiContent> contents { get; set; } = [];
}

public sealed class GeminiContent
{
    public string role { get; set; } = "user";
    public List<GeminiPart> parts { get; set; } = [];
}

public sealed class GeminiPart
{
    public string text { get; set; } = "";
}