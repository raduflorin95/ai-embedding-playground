using EmbeddingPlayground.Core.Abstractions;
using EmbeddingPlayground.Core.Models;
using EmbeddingPlayground.Intent.Helpers;
using System.Text.Json;

namespace EmbeddingPlayground.Intent.Services;

public sealed class IntentExtractor : IIntentExtractor
{
    private readonly ILlmService _llm;

    public IntentExtractor(ILlmService llm)
    {
        _llm = llm;
    }

    public async Task<IntentResult> ExtractAsync(string question)
    {
        var prompt =
            "You are an intent classifier.\n\n" +
            "Return JSON:\n" +
            "{\n" +
            "  \"intent\": \"...\",\n" +
            "  \"domain\": \"...\",\n" +
            "  \"normalizedQuery\": \"...\"\n" +
            "}\n\n" +
            "Domains: auth, billing, database, caching, messaging, networking, storage, observability\n\n" +
            $"Question:\n{question}";

        var raw = await _llm.AskAsync(prompt);
        var cleaned = LlmResponseCleaner.Clean(raw);

        return JsonSerializer.Deserialize<IntentResult>(cleaned)!;
    }
}