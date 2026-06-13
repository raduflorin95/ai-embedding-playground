using EmbeddingPlayground.Core.Models;

namespace EmbeddingPlayground.Core.Abstractions;

public interface IMemoryStore
{
    Task AddAsync(MemoryEntry entry, CancellationToken ct = default);

    Task<SearchResult?> FindBestMatchAsync(
            string query,
            float[] embedding,
            CancellationToken ct = default);
}