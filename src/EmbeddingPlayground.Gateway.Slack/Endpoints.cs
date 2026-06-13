using EmbeddingPlayground.Gateway.Slack.Models;
using EmbeddingPlayground.Gateway.Slack.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace EmbeddingPlayground.Gateway.Slack;

public static class Endpoints
{
    public static void MapEndpoints(this WebApplication app)
    {
        app.MapPost("/slack/events", async (
            HttpContext context,
            [FromServices] SlackDispatcher dispatcher) =>
        {
            var payloadText = await new StreamReader(context.Request.Body).ReadToEndAsync();
            var payloadJson = JsonSerializer.Deserialize<JsonElement> (payloadText);

            // Slack URL verification handshake
            if (payloadJson.GetProperty("type").GetString() == "url_verification")
            {
                return Results.Ok(payloadJson);
            }

            var payload = JsonSerializer.Deserialize<SlackEvent>(payloadText, new JsonSerializerOptions(JsonSerializerDefaults.Web));
            _ = dispatcher.HandleAsync(payload!);

            return Results.Ok();
        });
    }
}