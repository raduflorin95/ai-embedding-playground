namespace EmbeddingPlayground.Core.Models;

public sealed record MemoryEntry(
    string ClusterId,
    float[] Embedding,
    string Query,
    string Answer);