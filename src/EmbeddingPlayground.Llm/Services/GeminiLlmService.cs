using EmbeddingPlayground.Core.Abstractions;
using EmbeddingPlayground.Llm.Models;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace EmbeddingPlayground.Llm.Services;

public sealed class GeminiLlmService : ILlmService
{
    private static readonly string Schema = """
    {
      "type": "object",
      "description": "Classify the user intention and output a JSON response for the input: {UserInput}",
      "properties": {
        "intent": {
          "type": "string",
          "description": "The main intent of the user request. (e.g. get_user_info, troubleshooting, how_to, comparison, etc.)"
        },
        "domain": {
          "type": "string",
          "description": "Technical domain of the question (auth, billing, database, caching, messaging, networking, storage, observability, etc.)"
        },
        "normalizedQuery": {
          "type": "string",
          "description": "A cleaned and normalized version of the user question"
        },
        "answer": {
          "type": "string",
          "description": "A response to the user question based on the identified intent and domain."
        }
      },
      "required": ["intent", "domain", "normalizedQuery", "answer"],
      "additionalProperties": false
    }
    """;

    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public GeminiLlmService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _apiKey = "";

        _httpClient.BaseAddress =
            new Uri("https://generativelanguage.googleapis.com/");
    }

    public async Task<string> AskAsync(string prompt, CancellationToken ct = default)
    {
        var request = new GeminiRequest
        {
            contents =
            [
                new GeminiContent
                {
                    role = "user",
                    parts =
                    [
                        new GeminiPart { text = prompt }
                    ]
                }
            ]
        };

        using var req = new HttpRequestMessage(
            HttpMethod.Post,
            $"v1beta/models/gemini-2.5-flash:generateContent?key={_apiKey}")
        {
            Content = JsonContent.Create(request)
        };

        req.Headers.Accept.Add(
            new MediaTypeWithQualityHeaderValue("application/json"));

        using var res = await _httpClient.SendAsync(req, ct);

        var json = await res.Content.ReadFromJsonAsync<GeminiResponse>(cancellationToken: ct);

        var text =
            json?.candidates?
                .FirstOrDefault()?
                .content?
                .parts?
                .FirstOrDefault()?
                .text;

        return text ?? string.Empty;
    }
}