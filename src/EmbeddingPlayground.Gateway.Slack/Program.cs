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
builder.Services.AddHttpClient<ILlmService, GeminiLlmService>();
builder.Services.AddSingleton<Metrics>();
builder.Services.AddScoped<RagService>();
builder.Services.AddScoped<SlackDispatcher>();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapEndpoints();
app.Run();