namespace EmbeddingPlayground.Gateway.Slack.Models;

public class SlackRequest
{
    public string team_id { get; set; }
    public string channel_id { get; set; }
    public string user_id { get; set; }
    public string command { get; set; }
    public string text { get; set; }
    public string response_url { get; set; }
}

public class SlackEvent
{
    public string Type { get; set; }
    public SlackEventInner Event { get; set; }
}

public class SlackEventInner
{
    public string Type { get; set; }
    public string Text { get; set; }
    public string User { get; set; }
    public string Channel { get; set; }
}