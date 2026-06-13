using EmbeddingPlayground.Core.Abstractions;
using EmbeddingPlayground.Core.Models;
using EmbeddingPlayground.Core.Utils;
using FuzzySharp;

public sealed class InMemoryStore : IMemoryStore
{
    private readonly List<MemoryEntry> _entries = new();
    private readonly object _lock = new();

    private const float EmbeddingWeight = 0.75f;
    private const float FuzzyWeight = 0.25f;
    private const float Threshold = 0.92f;

    public Task AddAsync(MemoryEntry entry, CancellationToken ct = default)
    {
        lock (_lock)
        {
            _entries.Add(entry);
        }

        return Task.CompletedTask;
    }

    public Task<SearchResult?> FindBestMatchAsync(
        string query,
        float[] embedding,
        CancellationToken ct = default)
    {
        MemoryEntry? best = null;
        float bestScore = float.MinValue;

        lock (_lock)
        {
            foreach (var e in _entries)
            {
                // 1. embedding similarity (semantic)
                var embeddingScore = VectorMath.CosineSimilarity(
                    embedding,
                    e.Embedding);

                // 2. fuzzy similarity (typos / wording)
                var fuzzyScore = Fuzz.Ratio(query, e.Query) / 100f;

                // 3. hybrid score
                var score =
                    (embeddingScore * EmbeddingWeight) +
                    (fuzzyScore * FuzzyWeight);

                if (score > bestScore)
                {
                    bestScore = score;
                    best = e;
                }
            }
        }

        if (best is null || bestScore < Threshold)
            return Task.FromResult<SearchResult?>(null);

        return Task.FromResult<SearchResult?>(
            new SearchResult(best, bestScore));
    }
}