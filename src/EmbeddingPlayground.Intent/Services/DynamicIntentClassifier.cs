using EmbeddingPlayground.Core.Abstractions;
using EmbeddingPlayground.Core.Models;
using EmbeddingPlayground.Intent.Models;
using Microsoft.ML.OnnxRuntimeGenAI;
using System.Reflection;
using System.Text;
using System.Text.Json;

namespace EmbeddingPlayground.Intent.Services;

public sealed class DynamicIntentClassifier
{
    private readonly IEmbeddingService _embed;
    private readonly List<IntentCluster> _clusters = new();

    private readonly float _threshold;

    public DynamicIntentClassifier(IEmbeddingService embed, float threshold = 0.82f)
    {
        _embed = embed;
        _threshold = threshold;
    }

    public async Task<string> ClassifyAsync(float[] embeddingVector)
    {
        IntentCluster? best = null;
        float bestScore = -1;

        foreach (var c in _clusters)
        {
            var score = Cosine(embeddingVector, c.Centroid);

            if (score > bestScore)
            {
                bestScore = score;
                best = c;
            }
        }

        // ✔ existing cluster
        if (best != null && bestScore >= _threshold)
        {
            best.Add(embeddingVector);
            return best.Id;
        }

        // ❗ new cluster
        var newCluster = new IntentCluster();
        newCluster.Add(embeddingVector);

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

        return dot / (MathF.Sqrt(magA) * MathF.Sqrt(magB));
    }
}