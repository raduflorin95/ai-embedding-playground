namespace EmbeddingPlayground.Core.Utils;

public static class VectorMath
{
    public static float CosineSimilarity(
        ReadOnlySpan<float> a,
        ReadOnlySpan<float> b)
    {
        float dot = 0;
        float magA = 0;
        float magB = 0;

        for (var i = 0; i < a.Length; i++)
        {
            dot += a[i] * b[i];
            magA += a[i] * a[i];
            magB += b[i] * b[i];
        }

        return dot /
               (float)(Math.Sqrt(magA) *
                       Math.Sqrt(magB));
    }
}