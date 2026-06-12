namespace EmbeddingPlayground.Core.Models;

public sealed record IntentResult(
    string Intent,
    string Domain,
    string NormalizedQuery);