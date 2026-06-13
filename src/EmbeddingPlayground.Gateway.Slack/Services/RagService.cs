using EmbeddingPlayground.Core.Abstractions;
using EmbeddingPlayground.Core.Models;
using EmbeddingPlayground.Intent.Services;
using System.Collections;

namespace EmbeddingPlayground.Gateway.Slack.Services;

public sealed class RagService
{
    private readonly IQueryRewriter _queryRewriter;
    private readonly DynamicIntentClassifier _classifier;
    private readonly IEmbeddingService _embedding;
    private readonly IMemoryStore _memory;
    private readonly ILlmService _llm;

    public RagService(
        IQueryRewriter queryRewriter,
        DynamicIntentClassifier classifier,
        IEmbeddingService embedding,
        IMemoryStore memory,
        ILlmService llm)
    {
        _queryRewriter = queryRewriter;
        _classifier = classifier;
        _embedding = embedding;
        _memory = memory;
        _llm = llm;
    }

    public async Task<string> ProcessAsync(string question)
    {
        //question = await _queryRewriter.RewriteAsync(question);

        // 1. embed query (E5 ONNX)
        var (normalizedQuestion, vector) = await _embedding.GenerateAsync(question);

        // 2. dynamic clustering (intent discovery)
        var clusterId = await _classifier.ClassifyAsync(vector);

        // 3. search memory INSIDE cluster
        var cached = await _memory.FindBestMatchAsync(clusterId, vector);

        if (cached != null)
        {
            System.Console.WriteLine("CACHE HIT");
            return cached.Entry.Answer;
        }

        System.Console.WriteLine("LLM CALL");

        // 4. fallback to LLM
        var answer = await _llm.AskAsync(question);

        // 5. store result
        await _memory.AddAsync(new MemoryEntry(
            clusterId,
            vector,
            question,
            answer));

        return answer;
    }
}