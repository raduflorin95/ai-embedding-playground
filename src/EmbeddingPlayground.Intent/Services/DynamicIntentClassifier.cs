using EmbeddingPlayground.Core.Abstractions;
using EmbeddingPlayground.Intent.Models;

namespace EmbeddingPlayground.Intent.Services;

public sealed class DynamicIntentClassifier
{
    private readonly IEmbeddingService _embedding;
    private readonly List<IntentCluster> _clusters = new();
    private readonly float _threshold;

    public DynamicIntentClassifier(IEmbeddingService embedding, float threshold = 0.86f)
    {
        _embedding = embedding;
        _threshold = threshold;
    }

    public async Task<string> ClassifyAsync(string query)
    {
        var vector = await _embedding.GenerateAsync(query);

        IntentCluster? best = null;
        float bestScore = -1;

        foreach (var cluster in _clusters)
        {
            if (cluster.Centroid is null)
                continue;

            var score = Cosine(vector, cluster.Centroid);

            if (score > bestScore)
            {
                bestScore = score;
                best = cluster;
            }
        }

        // existing intent
        if (best is not null && bestScore >= _threshold)
        {
            best.Add(query, vector);
            return best.Id;
        }

        // new intent
        var newCluster = new IntentCluster(Guid.NewGuid().ToString("N"));
        newCluster.Add(query, vector);

        _clusters.Add(newCluster);

        return newCluster.Id;
    }

    private static float Cosine(float[] a, float[] b)
    {
        float dot = 0, magA = 0, magB = 0;

        for (int i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            magA += a[i] * a[i];
            magB += b[i] * b[i];
        }

        return dot / ((float)Math.Sqrt(magA) * (float)Math.Sqrt(magB));
    }
}