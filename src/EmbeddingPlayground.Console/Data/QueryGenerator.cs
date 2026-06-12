namespace EmbeddingPlayground.Console.Data;

public static class QueryGenerator
{
    private static readonly string[] QueryTemplates =
    [
        "How do I {action} {topic}?",
        "What is the best way to {action} {topic}?",
        "How can I troubleshoot {topic} issues?",
        "Explain {topic} configuration",
        "Why is my {topic} not working?",
        "How to fix problems with {topic}?",
        "Best practices for {topic} in production?",
        "How do I enable {topic}?",
        "How do I optimize {topic} performance?",
        "What causes failures in {topic}?"
    ];

    private static readonly string[] Actions =
    [
        "configure",
        "enable",
        "debug",
        "optimize",
        "secure",
        "scale",
        "integrate",
        "monitor"
    ];

    private static readonly string[] Topics =
    [
        "authentication tokens",
        "password reset flow",
        "refund processing",
        "rate limiting",
        "request retries",
        "data encryption",
        "log aggregation",
        "API keys",
        "session handling",
        "cache invalidation"
    ];

    public static List<(string Topic, string Query)> Generate(int count)
    {
        var rnd = new Random();
        var list = new List<(string, string)>();

        for (int i = 0; i < count; i++)
        {
            var template = QueryTemplates[rnd.Next(QueryTemplates.Length)];
            var action = Actions[rnd.Next(Actions.Length)];
            var topic = Topics[rnd.Next(Topics.Length)];

            var query = template
                .Replace("{action}", action)
                .Replace("{topic}", topic);

            list.Add((topic, query));
        }

        return list;
    }
}