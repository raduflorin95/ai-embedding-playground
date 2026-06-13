using EmbeddingPlayground.Console.Services;
using EmbeddingPlayground.Core.Abstractions;
using EmbeddingPlayground.Core.Models;
using EmbeddingPlayground.Embeddings.Services;
using EmbeddingPlayground.Gateway.Slack;
using EmbeddingPlayground.Gateway.Slack.Services;
using EmbeddingPlayground.Intent.Services;
using EmbeddingPlayground.Llm.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddSingleton<IMemoryStore, InMemoryStore>();
builder.Services.AddSingleton<IIntentExtractor, IntentExtractor>();
builder.Services.AddSingleton<IEmbeddingService, E5EmbeddingService>();
builder.Services.AddSingleton<DynamicIntentClassifier>();
//services.AddSingleton<IQueryRewriter, OnnxQueryRewriter>();
builder.Services.AddSingleton<QueryPipeline>();
builder.Services.AddHttpClient<ILlmService, GeminiLlmService>();
builder.Services.AddScoped<SlackDispatcher>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddAntiforgery();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseAntiforgery();
app.MapEndpoints();
app.Run();