using EmbeddingPlayground.Console.Engine;
using EmbeddingPlayground.Console.Options;
using EmbeddingPlayground.Console.Services;
using EmbeddingPlayground.Core.Abstractions;
using EmbeddingPlayground.Core.Models;
using EmbeddingPlayground.Embeddings.Services;
using EmbeddingPlayground.Intent.Services;
using EmbeddingPlayground.Llm.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.ML.OnnxRuntimeGenAI;

var services = new ServiceCollection();

services.AddSingleton<IMemoryStore, InMemoryStore>();
services.AddSingleton<IIntentExtractor, IntentExtractor>();
services.AddSingleton<IEmbeddingService, E5EmbeddingService>();
services.AddSingleton<DynamicIntentClassifier>();
//services.AddSingleton<IQueryRewriter, OnnxQueryRewriter>();
services.AddSingleton<QueryPipeline>();
services.AddHttpClient<ILlmService, GeminiLlmService>();

//services.AddSingleton<SemanticCacheService>();
services.AddSingleton<Metrics>();
services.AddSingleton(new SemanticCacheOptions { SimilarityThreshold = 0.90f });

var provider = services.BuildServiceProvider();

var embedding = provider.GetRequiredService<IEmbeddingService>();
var classifier = provider.GetRequiredService<DynamicIntentClassifier>();
var memory = provider.GetRequiredService<IMemoryStore>();
var llm = provider.GetRequiredService<ILlmService>();
var pipeline = provider.GetRequiredService<QueryPipeline>();


while (true)
{
    Console.WriteLine("Ask a question:");
    var question = Console.ReadLine();

    if (string.IsNullOrWhiteSpace(question))
        break;

    var answer = await pipeline.AskAsync(question);

    Console.WriteLine(answer);
}