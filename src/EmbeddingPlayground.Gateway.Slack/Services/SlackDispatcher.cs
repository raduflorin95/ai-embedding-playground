using EmbeddingPlayground.Console.Services;
using EmbeddingPlayground.Gateway.Slack.Models;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using static Lucene.Net.Util.Fst.Util;
using static System.Net.Mime.MediaTypeNames;

namespace EmbeddingPlayground.Gateway.Slack.Services;

public class SlackDispatcher
{
    private readonly QueryPipeline _pipeline;
    private readonly IHttpClientFactory _http;
    private readonly IConfiguration _config;

    public SlackDispatcher(
        QueryPipeline pipeline,
        IHttpClientFactory http,
        IConfiguration config)
    {
        _pipeline = pipeline;
        _http = http;
        _config = config;
    }

    public async Task HandleAsync(SlackRequest payload)
    {
        var question = Regex.Replace(payload.text, "<@[^>]+>", "").Trim();
        var channel = payload.channel_id;

        var answer = await _pipeline.AskAsync(question);

        await SendMessage(payload.user_id, question, channel, answer);
    }

    private async Task SendMessage(string userId, string question, string channel, string text)
    {
        var client = _http.CreateClient();

        var token = _config["Slack:BotToken"];

        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        await client.PostAsJsonAsync(
            "https://slack.com/api/chat.postMessage",
            new
            {
                response_type = "in_channel",
                channel,
                text = $"<@{userId}> asked: {question}\n\nAnswer:\n\n{text}"
            });
    }
}