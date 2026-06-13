namespace EmbeddingPlayground.Core.Abstractions;

public interface IQueryRewriter
{
    Task<string> RewriteAsync(string query);
}