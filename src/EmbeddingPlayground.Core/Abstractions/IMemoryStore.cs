using EmbeddingPlayground.Core.Models;

namespace EmbeddingPlayground.Core.Abstractions;

public interface IMemoryStore
{
    Task AddAsync(
        MemoryEntry entry,
        CancellationToken cancellationToken = default);

    Task<SearchResult?> FindBestMatchAsync(
        float[] embedding,
        CancellationToken cancellationToken = default);
}