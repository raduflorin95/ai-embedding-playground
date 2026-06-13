using EmbeddingPlayground.Core.Abstractions;
using EmbeddingPlayground.Core.Models;
using EmbeddingPlayground.Intent.Services;

namespace EmbeddingPlayground.Console.Services;

public sealed class QueryPipeline
{
    private readonly IQueryRewriter _queryRewriter;
    private readonly IEmbeddingService _embedding;
    private readonly DynamicIntentClassifier _classifier;
    private readonly IMemoryStore _memory;
    private readonly ILlmService _llm;

    public QueryPipeline(
        //IQueryRewriter queryRewriter,
        IEmbeddingService embedding,
        DynamicIntentClassifier classifier,
        IMemoryStore memory,
        ILlmService llm)
    {
        //_queryRewriter = queryRewriter;
        _embedding = embedding;
        _classifier = classifier;
        _memory = memory;
        _llm = llm;
    }

    public async Task<string> AskAsync(string query)
    {
        //query = await _queryRewriter.RewriteAsync(query);
        
        // 1. embed query (E5 ONNX)
        var (normalizedQuery, vector) = await _embedding.GenerateAsync(query);

        // 2. dynamic clustering (intent discovery)
        var clusterId = await _classifier.ClassifyAsync(vector);

        // 3. search memory INSIDE cluster
        var cached = await _memory.FindBestMatchAsync(normalizedQuery, vector);

        if (cached != null)
        {
            System.Console.WriteLine("CACHE HIT");
            return cached.Entry.Answer;
        }

        System.Console.WriteLine("LLM CALL");

        // 4. fallback to LLM
        var answer = await _llm.AskAsync(query);

        // 5. store result
        await _memory.AddAsync(new MemoryEntry(
            clusterId,
            vector,
            query,
            answer));

        return answer;
    }
}