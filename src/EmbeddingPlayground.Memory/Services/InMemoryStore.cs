using EmbeddingPlayground.Core.Abstractions;
using EmbeddingPlayground.Core.Models;
using EmbeddingPlayground.Core.Utils;

public sealed class InMemoryStore : IMemoryStore
{
    private readonly List<MemoryEntry> _entries = new();
    private readonly object _lock = new();

    public Task AddAsync(MemoryEntry entry, CancellationToken ct = default)
    {
        lock (_lock)
        {
            _entries.Add(entry);
        }

        return Task.CompletedTask;
    }

    public Task<SearchResult?> FindBestMatchAsync(
        float[] embedding,
        CancellationToken ct = default)
    {
        MemoryEntry? best = null;
        float bestScore = float.MinValue;

        lock (_lock)
        {
            foreach (var e in _entries)
            {
                var score = VectorMath.CosineSimilarity(
                    embedding,
                    e.Embedding);

                if (score > bestScore)
                {
                    bestScore = score;
                    best = e;
                }
            }
        }

        return Task.FromResult(
            best is null
                ? null
                : new SearchResult(best, bestScore));
    }
}