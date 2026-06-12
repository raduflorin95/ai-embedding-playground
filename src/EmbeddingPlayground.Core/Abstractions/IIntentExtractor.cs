using EmbeddingPlayground.Core.Models;

namespace EmbeddingPlayground.Core.Abstractions;

public interface IIntentExtractor
{
    Task<IntentResult> ExtractAsync(string question);
}