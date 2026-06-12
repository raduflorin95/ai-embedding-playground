namespace EmbeddingPlayground.Intent.Helpers;

public static class LlmResponseCleaner
{
    public static string Clean(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return input;

        var text = input.Trim();

        // Remove ```json or ``` blocks
        if (text.StartsWith("```"))
        {
            var firstNewline = text.IndexOf('\n');
            var lastFence = text.LastIndexOf("```");

            if (firstNewline >= 0 && lastFence > firstNewline)
            {
                text = text.Substring(firstNewline, lastFence - firstNewline);
            }
        }

        return text.Trim();
    }
}