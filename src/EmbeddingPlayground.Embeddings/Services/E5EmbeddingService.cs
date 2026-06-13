using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.Tokenizers;
using System.Text.RegularExpressions;


namespace EmbeddingPlayground.Embeddings.Services;

using EmbeddingPlayground.Core.Abstractions;
using EmbeddingPlayground.Embeddings.Helpers;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.Tokenizers;
using System.Text.RegularExpressions;

public sealed class WordPieceTokenizer
{
    private readonly Dictionary<string, int> _vocab;
    private readonly int _unkId;

    public WordPieceTokenizer(string vocabPath)
    {
        _vocab = File.ReadAllLines(vocabPath)
            .Select((w, i) => (w, i))
            .ToDictionary(x => x.w, x => x.i);

        _unkId = _vocab["[UNK]"];
    }

    public List<int> Encode(string text)
    {
        text = text.ToLowerInvariant();
        var tokens = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        var ids = new List<int>();

        foreach (var token in tokens)
        {
            if (_vocab.TryGetValue(token, out var id))
                ids.Add(id);
            else
                ids.Add(_unkId);
        }

        return ids;
    }
}

public sealed class MiniLmEmbeddingService
{
    private readonly InferenceSession _session;
    private readonly WordPieceTokenizer _tokenizer;

    public MiniLmEmbeddingService(string modelPath, string vocabPath)
    {
        _session = new InferenceSession(Path.Combine(modelPath, "model.onnx"));
        _tokenizer = new WordPieceTokenizer(vocabPath);
    }

    public float[] Embed(string text)
    {
        var inputIds = _tokenizer.Encode(text);

        var ids = inputIds.Select(x => (long)x).ToArray();
        var mask = Enumerable.Repeat(1L, ids.Length).ToArray();
        var tokenTypeIds = new DenseTensor<long>(
            Enumerable.Repeat(0L, ids.Length).ToArray(),
            new[] { 1, ids.Length });

        var inputs = new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("input_ids",
                new DenseTensor<long>(ids, new[] { 1, ids.Length })),

            NamedOnnxValue.CreateFromTensor("attention_mask",
                new DenseTensor<long>(mask, new[] { 1, mask.Length })),

            NamedOnnxValue.CreateFromTensor("token_type_ids", tokenTypeIds)
        };

        using var results = _session.Run(inputs);

        var hidden = results
            .First(x => x.Name == "last_hidden_state")
            .AsTensor<float>();

        return MeanPool(hidden);
    }

    private static float[] MeanPool(Tensor<float> hidden)
    {
        int seq = hidden.Dimensions[1];
        int dim = hidden.Dimensions[2];

        var v = new float[dim];

        for (int i = 0; i < seq; i++)
            for (int j = 0; j < dim; j++)
                v[j] += hidden[0, i, j];

        for (int j = 0; j < dim; j++)
            v[j] /= seq;

        return Normalize(v);
    }

    private static float[] Normalize(float[] v)
    {
        double sum = 0;

        for (int i = 0; i < v.Length; i++)
            sum += v[i] * v[i];

        var norm = Math.Sqrt(sum);

        for (int i = 0; i < v.Length; i++)
            v[i] /= (float)norm;

        return v;
    }
}
public sealed class E5EmbeddingService : IEmbeddingService, IDisposable
{
    private readonly InferenceSession _session;
    private readonly Tokenizer _tokenizer;
    private readonly MiniLmEmbeddingService _miniLmEmbeddingService;

    private const int MaxLen = 512;

    public E5EmbeddingService()
    {
        var modelPath = Path.Combine(
            AppContext.BaseDirectory,
            "Resources",
            "all-MiniLM-L6-v2");

        var tokenizerPath = Path.Combine(
            AppContext.BaseDirectory,
            "Resources",
            "all-MiniLM-L6-v2",
            "vocab.txt");

        _miniLmEmbeddingService = new MiniLmEmbeddingService(modelPath, tokenizerPath);

        //_session = new InferenceSession(
        //    Path.Combine(modelPath, "model.onnx"));
    }

    public Task<(string normalizedText, float[] embeddingsVector)> GenerateAsync(string text, bool isQuery = true)
    {
        var analyzer = QueryAnalyzerFactory.Create();
        var normalized = LuceneNormalizer.Normalize(text, analyzer);

        return Task.FromResult((normalized, _miniLmEmbeddingService.Embed(normalized)));

        //text = Normalize(text);

        //// IMPORTANT: E5 format
        //text = isQuery
        //    ? $"query: {text}"
        //    : $"passage: {text}";

        //var tokenIds = _tokenizer.Encode(text).Ids;

        //if (tokenIds.Count > MaxLen)
        //    tokenIds = tokenIds.Take(MaxLen).ToList();

        //var inputIds = tokenIds.Select(x => (long)x).ToArray();
        //var attentionMask = Enumerable.Repeat(1L, inputIds.Length).ToArray();

        //var inputs = new List<NamedOnnxValue>
        //{
        //    NamedOnnxValue.CreateFromTensor(
        //        "input_ids",
        //        new DenseTensor<long>(inputIds, new[] { 1, inputIds.Length })),

        //    NamedOnnxValue.CreateFromTensor(
        //        "attention_mask",
        //        new DenseTensor<long>(attentionMask, new[] { 1, attentionMask.Length }))
        //};

        //using var results = _session.Run(inputs);

        //var hidden = results
        //    .First(x => x.Name == "last_hidden_state")
        //    .AsTensor<float>();

        //return MeanPool(hidden, attentionMask);
    }

    // -----------------------------
    // Core pooling logic
    // -----------------------------
    private static float[] MeanPool(Tensor<float> hidden, long[] mask)
    {
        int seqLen = hidden.Dimensions[1];
        int dim = hidden.Dimensions[2];

        var embedding = new float[dim];

        int count = 0;

        for (int i = 0; i < seqLen; i++)
        {
            if (mask[i] == 0)
                continue;

            count++;

            for (int j = 0; j < dim; j++)
                embedding[j] += hidden[0, i, j];
        }

        if (count > 0)
        {
            for (int j = 0; j < dim; j++)
                embedding[j] /= count;
        }

        return Normalize(embedding);
    }

    // -----------------------------
    // Normalization (critical)
    // -----------------------------
    private static float[] Normalize(float[] vector)
    {
        double sum = 0;

        for (int i = 0; i < vector.Length; i++)
            sum += vector[i] * vector[i];

        var norm = Math.Sqrt(sum);

        if (norm == 0)
            return vector;

        for (int i = 0; i < vector.Length; i++)
            vector[i] /= (float)norm;

        return vector;
    }

    // -----------------------------
    // Text preprocessing (VERY important)
    // -----------------------------
    private static string Normalize(string text)
    {
        text = text.ToLowerInvariant();
        text = text.Trim();
        text = Regex.Replace(text, @"\s+", " ");
        text = Regex.Replace(text, @"[^\w\s]", ""); // remove punctuation
        return text;
    }

    public void Dispose()
    {
        _session.Dispose();
    }
}