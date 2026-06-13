using EmbeddingPlayground.Core.Abstractions;
using Microsoft.ML.OnnxRuntimeGenAI;

namespace EmbeddingPlayground.Intent.Services;

public sealed class OnnxQueryRewriter : IQueryRewriter
{
    private readonly Model _model;
    private readonly Tokenizer _tokenizer;

    public OnnxQueryRewriter()
    {
        var modelPath = Path.Combine(
            AppContext.BaseDirectory,
            "Resources",
            "e5-small-v2");
        _model = new Model(modelPath);
        _tokenizer = new Tokenizer(_model);
    }

    public async Task<string> RewriteAsync(string query)
    {
        var prompt = BuildPrompt(query);

        var tokens = _tokenizer.Encode(prompt);

        using var generatorParams = new GeneratorParams(_model);
        generatorParams.SetSearchOption("max_length", 264);
        generatorParams.SetSearchOption("temperature", 0.0);
        generatorParams.SetSearchOption("top_k", 1);
        generatorParams.SetSearchOption("top_p", 1.0);
        generatorParams.SetSearchOption("repetition_penalty", 1.1);

        using var generator = new Generator(_model, generatorParams);

        generator.AppendTokens(tokens[0]);

        while (!generator.IsDone())
        {
            generator.GenerateNextToken();
        }

        var sequence = generator.GetSequence(0);
        var output = _tokenizer.Decode(sequence);

        return ExtractFirstLine(output);
    }

    private static string BuildPrompt(string query)
    {
        return $"""
Rewrite queries into clean search queries.

Examples:
who is peter griffin? → peter griffin
who is michael jackson? → michael jackson

{query} →
""";
    }

    private static string ExtractFirstLine(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        return text
            .Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .First()
            .Trim();
    }
}