namespace EmbeddingPlayground.Intent.Models;

public sealed class IntentCluster
{
    public string Id { get; }

    public List<string> Samples { get; } = new();
    public List<float[]> Vectors { get; } = new();

    public float[]? Centroid { get; private set; }

    public IntentCluster(string id)
    {
        Id = id;
    }

    public void Add(string sample, float[] vector)
    {
        Samples.Add(sample);
        Vectors.Add(vector);

        RecalculateCentroid();
    }

    private void RecalculateCentroid()
    {
        if (Vectors.Count == 0)
        {
            Centroid = null;
            return;
        }

        var dim = Vectors[0].Length;
        var sum = new float[dim];

        foreach (var v in Vectors)
        {
            for (int i = 0; i < dim; i++)
            {
                sum[i] += v[i];
            }
        }

        for (int i = 0; i < dim; i++)
        {
            sum[i] /= Vectors.Count;
        }

        Centroid = sum;
    }
}