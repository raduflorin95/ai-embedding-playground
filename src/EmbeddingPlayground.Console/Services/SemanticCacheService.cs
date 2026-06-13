//using EmbeddingPlayground.Console.Options;
//using EmbeddingPlayground.Core.Abstractions;
//using EmbeddingPlayground.Core.Models;

//namespace EmbeddingPlayground.Console.Services;

//public sealed class SemanticCacheService
//{
//    private readonly IIntentExtractor _intentExtractor;
//    private readonly IEmbeddingService _embeddingService;
//    private readonly IMemoryStore _memoryStore;
//    private readonly ILlmService _llm;
//    private readonly Metrics _metrics;
//    private readonly SemanticCacheOptions _options;

//    public SemanticCacheService(
//        IIntentExtractor intentExtractor,
//        IEmbeddingService embeddingService,
//        IMemoryStore memoryStore,
//        ILlmService llm,
//        Metrics metrics,
//        SemanticCacheOptions options)
//    {
//        _intentExtractor = intentExtractor;
//        _embeddingService = embeddingService;
//        _memoryStore = memoryStore;
//        _llm = llm;
//        _metrics = metrics;
//        _options = options;
//    }

//    public async Task<string> AskAsync(
//        string question,
//        CancellationToken cancellationToken = default)
//    {
//        _metrics.TotalQuestions++;

//        var intent = await _intentExtractor.ExtractAsync(question);
//        var queryEmbedding = await _embeddingService.GenerateAsync(intent.NormalizedQuery, cancellationToken);

//        var match = await _memoryStore.FindBestMatchAsync(
//            queryEmbedding,
//            intent.Intent,
//            threshold: 0.85f,
//            cancellationToken);

//        if (match is not null &&
//            match.Similarity >= _options.SimilarityThreshold)
//        {
//            _metrics.RetrievalHits++;
//            return match.Entry.Answer;
//        }

//        _metrics.LlmCalls++;

//        var answer = await _llm.AskAsync(question, cancellationToken);
//        queryEmbedding = await _embeddingService.GenerateAsync(question, cancellationToken);

//        var entry = new MemoryEntry(
//            Id: Guid.NewGuid(),
//            Question: question,
//            Answer: answer,
//            Intent: intent.Intent,
//            Embedding: queryEmbedding,
//            CreatedUtc: DateTime.UtcNow);

//        await _memoryStore.AddAsync(entry, cancellationToken);

//        return answer;
//    }
//}