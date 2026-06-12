using EmbeddingPlayground.Console.Engine;
using EmbeddingPlayground.Console.Options;
using EmbeddingPlayground.Console.Services;
using EmbeddingPlayground.Core.Abstractions;
using EmbeddingPlayground.Core.Models;
using EmbeddingPlayground.Embeddings.Services;
using EmbeddingPlayground.Intent.Services;
using EmbeddingPlayground.Llm.Services;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddSingleton<IMemoryStore, InMemoryStore>();
services.AddSingleton<IIntentExtractor, IntentExtractor>();
services.AddSingleton<IEmbeddingService, E5EmbeddingService>();
services.AddSingleton<DynamicIntentClassifier>();
services.AddHttpClient<ILlmService, GeminiLlmService>();

services.AddSingleton<SemanticCacheService>();
services.AddSingleton<Metrics>();
services.AddSingleton(new SemanticCacheOptions { SimilarityThreshold = 0.90f });

var provider = services.BuildServiceProvider();

var embedding = provider.GetRequiredService<IEmbeddingService>();
var classifier = provider.GetRequiredService<DynamicIntentClassifier>();
var memory = provider.GetRequiredService<IMemoryStore>();
var llm = provider.GetRequiredService<ILlmService>();

while (true)
{
    Console.WriteLine("Ask a question:");
    var question = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(question))
        break;

    //
    // 1. STEP: dynamic intent discovery
    //
    var intentId = await classifier.ClassifyAsync(question);

    Console.WriteLine($"Intent: {intentId}");

    //
    // 2. STEP: embed question (or normalized cluster meaning)
    //
    var vector = await embedding.GenerateAsync(question);

    //
    // 3. STEP: semantic cache lookup
    //
    var match = await memory.FindBestMatchAsync(vector);

    if (match is not null && match.Similarity > 0.88f)
    {
        Console.WriteLine("CACHE HIT:");
        Console.WriteLine(match.Entry.Answer);
        continue;
    }

    //
    // 4. STEP: LLM fallback (Gemini)
    //
    Console.WriteLine("LLM CALL...");

    var answer = await llm.AskAsync(question);

    //
    // 5. STEP: store in memory under intent cluster
    //
    await memory.AddAsync(new MemoryEntry(
        Guid.NewGuid(),
        intentId,
        answer,
        vector,
        DateTime.UtcNow));

    Console.WriteLine(answer);
}