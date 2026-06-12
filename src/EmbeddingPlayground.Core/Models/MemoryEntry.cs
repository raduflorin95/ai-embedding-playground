namespace EmbeddingPlayground.Core.Models;

public sealed record MemoryEntry(
    Guid Id,
    string Question,
    string Answer,
    float[] Embedding,
    DateTime CreatedUtc);