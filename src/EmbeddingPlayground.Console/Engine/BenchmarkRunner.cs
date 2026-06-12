using EmbeddingPlayground.Console.Data;
using EmbeddingPlayground.Console.Services;
using EmbeddingPlayground.Core.Models;

namespace EmbeddingPlayground.Console.Engine;

public sealed class BenchmarkRunner
{
    private readonly SemanticCacheService _cache;
    private readonly Metrics _metrics;

    public BenchmarkRunner(
        SemanticCacheService cache,
        Metrics metrics)
    {
        _cache = cache;
        _metrics = metrics;
    }

    public async Task RunAsync()
    {
        System.Console.WriteLine("=== BENCHMARK START ===");

        var baseQueries = QueryGenerator.Generate(10).ToList();

        var noisyQueries = baseQueries
            .Select(q =>
            {
                q.Query = QueryNoise.AddNoise(q.Query);
                return q;
            })
            .ToList();

        foreach (var (intent, query) in noisyQueries)
        {
            var answer = await _cache.AskAsync(query);

            System.Console.WriteLine($"{intent} | {query}");
        }

        PrintResults();
    }

    private void PrintResults()
    {
        System.Console.WriteLine("\n=== RESULTS ===");

        System.Console.WriteLine($"Total: {_metrics.TotalQuestions}");
        System.Console.WriteLine($"Hits: {_metrics.RetrievalHits}");
        System.Console.WriteLine($"LLM Calls: {_metrics.LlmCalls}");

        var hitRate =
            (double)_metrics.RetrievalHits /
            _metrics.TotalQuestions;

        var savings =
            1.0 - ((double)_metrics.LlmCalls /
                   _metrics.TotalQuestions);

        System.Console.WriteLine($"Hit Rate: {hitRate:P2}");
        System.Console.WriteLine($"LLM Savings: {savings:P2}");
    }
}