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

        var answer = await _llm.AskAsync(prompt);
        var cleaned = LlmResponseCleaner.Clean(answer);

        return JsonSerializer.Deserialize<IntentResult>(prompt)!;
    }

    public async Task<(string intent, Dictionary<string, string> slots)> ExtractSlotsAsync(string question)
    {
        var prompt = """
        Extract structured intent + slots.

        Return JSON only:

        {{
          "intent": "who_is | how_to | fact_query | comparison | etc...",
          "slots": {{
            "subject": "..."
          }}
        }}

        Question:
        {question}
        """;
        prompt = prompt.Replace("{question}", question);

        var json = await _llm.AskAsync(prompt);

        json = json
            .Replace("```json", "")
            .Replace("```", "")
            .Trim();

        using var doc = JsonDocument.Parse(json);

        var intent = doc.RootElement.GetProperty("intent").GetString()!;
        var slots = new Dictionary<string, string>();

        foreach (var prop in doc.RootElement.GetProperty("slots").EnumerateObject())
        {
            slots[prop.Name] = prop.Value.GetString() ?? "";
        }

        return (intent, slots);
    }
}