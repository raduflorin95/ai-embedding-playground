namespace EmbeddingPlayground.Core.Models;

public sealed record SearchResult(
    MemoryEntry Entry,
    float Similarity);