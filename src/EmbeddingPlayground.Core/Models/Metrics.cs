namespace EmbeddingPlayground.Core.Models;

public sealed class Metrics
{
    public int TotalQuestions { get; set; }

    public int RetrievalHits { get; set; }

    public int LlmCalls { get; set; }
}