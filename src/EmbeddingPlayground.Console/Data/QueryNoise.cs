namespace EmbeddingPlayground.Console.Data;

public static class QueryNoise
{
    private static readonly string[] Noise =
    [
        "",
        " urgently",
        " pls help",
        " in production",
        " best practice",
        " error happening",
        " not working",
        " suddenly failing"
    ];

    public static string AddNoise(string query)
    {
        var rnd = new Random();
        return query + Noise[rnd.Next(Noise.Length)];
    }
}