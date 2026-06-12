using EmbeddingPlayground.Core.Abstractions;
using EmbeddingPlayground.Embeddings.Models;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using Microsoft.ML.Tokenizers;

namespace EmbeddingPlayground.Embeddings.Services;

public sealed class E5EmbeddingService : IEmbeddingService
{
    private readonly InferenceSession _session;
    private readonly Tokenizer _tokenizer;

    public E5EmbeddingService()
    {
        var modelPath = Path.Combine(
            AppContext.BaseDirectory,
            "Resources",
            "model_qint8_avx512_vnni.onnx");

        var tokenizerPath = Path.Combine(
            AppContext.BaseDirectory,
            "Resources",
            "tokenizer.json");

        _session = new InferenceSession(modelPath);

        var vocabPath = Path.Combine(
            AppContext.BaseDirectory,
            "Resources",
            "vocab.txt");
        _tokenizer = BertTokenizer.Create(vocabPath);
    }

    public async Task<float[]> GenerateAsync(
        string text,
        CancellationToken cancellationToken = default)
    {
        text = $"query: {text}";

        var tokenized = Tokenize(text);

        var inputs = CreateInputs(tokenized);

        using var results = _session.Run(inputs);

        var output = results
            .First(x => x.Name == "last_hidden_state")
            .AsTensor<float>();

        return MeanPoolAndNormalize(
            output,
            tokenized.AttentionMask);
    }

    #region Private methods
    private List<NamedOnnxValue> CreateInputs(TokenizedInput tokenized)
    {
        var inputIds = new DenseTensor<long>(
            tokenized.InputIds,
            new[] { 1, tokenized.InputIds.Length });

        var attentionMask = new DenseTensor<long>(
            tokenized.AttentionMask,
            new[] { 1, tokenized.AttentionMask.Length });

        var tokenTypeIds = new DenseTensor<long>(
            Enumerable.Repeat(0L, tokenized.InputIds.Length).ToArray(),
            new[] { 1, tokenized.InputIds.Length });

        return new List<NamedOnnxValue>
        {
            NamedOnnxValue.CreateFromTensor("input_ids", inputIds),
            NamedOnnxValue.CreateFromTensor("attention_mask", attentionMask),
            NamedOnnxValue.CreateFromTensor("token_type_ids", tokenTypeIds)
        };
    }

    private TokenizedInput Tokenize(string text)
    {
        const int maxLen = 512;

        // E5 expects prefix
        text = $"query: {text}";

        var encoded = _tokenizer.EncodeToIds(text);

        var inputIds = encoded
            .Take(maxLen)
            .Select(x => (long)x)
            .ToArray();

        var attentionMask = new long[inputIds.Length];

        for (int i = 0; i < attentionMask.Length; i++)
            attentionMask[i] = 1;

        return new TokenizedInput
        {
            InputIds = inputIds,
            AttentionMask = attentionMask
        };
    }

    private static float[] MeanPoolAndNormalize(
        Tensor<float> hiddenStates,
        long[] attentionMask)
    {
        int sequenceLength =
            hiddenStates.Dimensions[1];

        int hiddenSize =
            hiddenStates.Dimensions[2];

        var embedding =
            new float[hiddenSize];

        long validTokens = 0;

        for (int token = 0;
             token < sequenceLength;
             token++)
        {
            if (attentionMask[token] == 0)
                continue;

            validTokens++;

            for (int dim = 0;
                 dim < hiddenSize;
                 dim++)
            {
                embedding[dim] +=
                    hiddenStates[0, token, dim];
            }
        }

        if (validTokens > 0)
        {
            for (int dim = 0;
                 dim < hiddenSize;
                 dim++)
            {
                embedding[dim] /=
                    validTokens;
            }
        }

        Normalize(embedding);

        return embedding;
    }

    private static void Normalize(
        float[] vector)
    {
        double magnitude = 0;

        for (int i = 0;
             i < vector.Length;
             i++)
        {
            magnitude +=
                vector[i] * vector[i];
        }

        magnitude = Math.Sqrt(magnitude);

        if (magnitude == 0)
            return;

        for (int i = 0;
             i < vector.Length;
             i++)
        {
            vector[i] /=
                (float)magnitude;
        }
    }
    #endregion
}