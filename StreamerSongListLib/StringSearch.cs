public class StringSearch
{
    // Extracts unique 3-character substrings from a text
    public static HashSet<string> ExtractTrigrams(string text)
    {
        var trigrams = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(text) || text.Length < 3)
        {
            return trigrams;
        }

        // Standard padding helps capture word boundaries effectively
        string padded = $"  {text.Trim()} ";

        for (int i = 0; i <= padded.Length - 3; i++)
        {
            trigrams.Add(padded.Substring(i, 3));
        }

        return trigrams;
    }

    // Calculates similarity using the Jaccard index formula: (A ∩ B) / (A ∪ B)
    public static double CalculateSimilarity(HashSet<string> targetGrams, HashSet<string> queryGrams)
    {
        if (targetGrams.Count == 0 || queryGrams.Count == 0) return 0.0;

        int intersectionCount = targetGrams.Count(queryGrams.Contains);
        int unionCount = targetGrams.Count + queryGrams.Count - intersectionCount;

        return (double)intersectionCount / unionCount;
    }

    //// Searches a corpus and returns matches ranked by highest similarity score
    //public static List<(string Text, double Score)> Search(List<string> corpus, string query, double threshold = 0.2)
    //{
    //    var queryGrams = ExtractTrigrams(query);
    //    var results = new List<(string Text, double Score)>();

    //    if (queryGrams.Count == 0) return results;

    //    for (int i = 0; i < corpus.Count; ++i)
    //    {
    //        var item = corpus[i];

    //        var itemGrams = ExtractTrigrams(item);
    //        double score = CalculateSimilarity(itemGrams, queryGrams);

    //        if (score >= threshold)
    //        {
    //            results.Add((item, score));
    //        }
    //    }

    //    return results.OrderByDescending(r => r.Score).ToList();
    //}

    // Searches a corpus and returns best matche index
    public static int GetBestMatch(string query, List<string> corpus, double threshold = 0.2)
    {
        var queryGrams = ExtractTrigrams(query);

        double bestScore = double.NegativeInfinity;
        int bestIndex = -1;

        if (queryGrams.Count == 0) return -1;

        for (int i = 0; i < corpus.Count; ++i)
        {
            var item = corpus[i];

            var itemGrams = ExtractTrigrams(item);
            double score = CalculateSimilarity(itemGrams, queryGrams);

            if ((score >= threshold) && (score > bestScore))
            {
                bestScore = score;
                bestIndex = i;
            }
        }

        return bestIndex;
    }
}