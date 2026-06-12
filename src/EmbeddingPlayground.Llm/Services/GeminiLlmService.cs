using EmbeddingPlayground.Core.Abstractions;
using EmbeddingPlayground.Llm.Models;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace EmbeddingPlayground.Llm.Services;

public sealed class GeminiLlmService : ILlmService
{
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