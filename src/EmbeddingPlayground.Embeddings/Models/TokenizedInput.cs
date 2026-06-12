namespace EmbeddingPlayground.Embeddings.Models;

internal sealed class TokenizedInput
{
    public required long[] InputIds { get; init; }

    public required long[] AttentionMask { get; init; }
}