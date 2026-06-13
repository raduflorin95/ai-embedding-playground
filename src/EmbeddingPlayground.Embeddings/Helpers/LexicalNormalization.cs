using Lucene.Net.Analysis;
using Lucene.Net.Analysis.En;
using Lucene.Net.Analysis.TokenAttributes;
using Lucene.Net.Util;

namespace EmbeddingPlayground.Embeddings.Helpers;

public static class LuceneNormalizer
{
    public static string Normalize(string text, Analyzer analyzer)
    {
        using var reader = new StringReader(text);
        using var tokenStream = analyzer.GetTokenStream("field", reader);

        var termAttr = tokenStream.GetAttribute<ICharTermAttribute>();

        tokenStream.Reset();

        var tokens = new List<string>();

        while (tokenStream.IncrementToken())
        {
            tokens.Add(termAttr.ToString());
        }

        tokenStream.End();

        return string.Join(" ", tokens);
    }
}

public static class QueryAnalyzerFactory
{
    public static Analyzer Create()
    {
        return new EnglishAnalyzer(LuceneVersion.LUCENE_48);
    }
}