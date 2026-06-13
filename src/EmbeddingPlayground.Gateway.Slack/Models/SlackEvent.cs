namespace EmbeddingPlayground.Gateway.Slack.Models;

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