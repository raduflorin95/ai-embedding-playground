using EmbeddingPlayground.Core.Models;

namespace EmbeddingPlayground.Core.Abstractions;

public interface IIntentExtractor
{
    Task<(string intent, Dictionary<string, string> slots)> ExtractSlotsAsync(string question);
    Task<IntentResult> ExtractAsync(string question);
}