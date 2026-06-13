namespace EmbeddingPlayground.Core.Abstractions;

public interface IEmbeddingService
{
    Task<(string normalizedText, float[] embeddingsVector)> GenerateAsync(string text, bool isQuery = true);
}