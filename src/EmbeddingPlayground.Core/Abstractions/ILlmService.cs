namespace EmbeddingPlayground.Core.Abstractions;

public interface ILlmService
{
    Task<string> AskAsync(
        string question,
        CancellationToken cancellationToken = default);
}