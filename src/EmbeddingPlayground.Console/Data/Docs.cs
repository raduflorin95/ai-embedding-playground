namespace EmbeddingPlayground.Console.Data;

public sealed record Doc(
    string Id,
    string Domain,
    string Title,
    string Content);