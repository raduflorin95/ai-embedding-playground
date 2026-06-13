using EmbeddingPlayground.Gateway.Slack.Models;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

namespace EmbeddingPlayground.Gateway.Slack.Services;

public class SlackDispatcher
{
    private readonly RagService _rag;
    private readonly IHttpClientFactory _http;
    private readonly IConfiguration _config;

    public SlackDispatcher(
        RagService rag,
        IHttpClientFactory http,
        IConfiguration config)
    {
        _rag = rag;
        _http = http;
        _config = config;
    }

    public async Task HandleAsync(SlackEvent payload)
    {
        var question = Regex.Replace(payload.Event.Text, "<@[^>]+>", "").Trim();
        var channel = payload.Event.Channel;

        var answer = await _rag.ProcessAsync(question);

        await SendMessage(channel, answer);
    }

    private async Task SendMessage(string channel, string text)
    {
        var client = _http.CreateClient();

        var token = _config["Slack:BotToken"];

        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        await client.PostAsJsonAsync(
            "https://slack.com/api/chat.postMessage",
            new
            {
                channel,
                text
            });
    }
}