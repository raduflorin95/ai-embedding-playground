namespace EmbeddingPlayground.Intent.Models;

public sealed class IntentCluster
{
    public string Id { get; } = Guid.NewGuid().ToString("N");

    public List<float[]> Vectors { get; } = new();

    public float[] Centroid { get; private set; }

    public void Add(float[] vector)
    {
        Vectors.Add(vector);
        Centroid = RecomputeCentroid();
    }

    private float[] RecomputeCentroid()
    {
        var dim = Vectors[0].Length;
        var result = new float[dim];

        foreach (var v in Vectors)
        {
            for (int i = 0; i < dim; i++)
                result[i] += v[i];
        }

        for (int i = 0; i < dim; i++)
            result[i] /= Vectors.Count;

        return result;
    }
}