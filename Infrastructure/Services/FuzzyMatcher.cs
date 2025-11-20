using Castle.Core.Logging;
using Core.Ports;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class FuzzyMatcher: IFuzzyMatcher
    {
        public int CalculateEditDistance(string s1, string s2)
        {
            if (string.IsNullOrEmpty(s1))
                return string.IsNullOrEmpty(s2) ? 0 : s2.Length;

            if (string.IsNullOrEmpty(s2))
                return s1.Length;

            int n = s1.Length;
            int m = s2.Length;
            int[,] d = new int[n + 1, m + 1];

            // Initialize first row and column
            for (int i = 0; i <= n; i++)
                d[i, 0] = i;

            for (int j = 0; j <= m; j++)
                d[0, j] = j;

            // Fill the matrix
            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = s1[i - 1] == s2[j - 1] ? 0 : 1;

                    d[i, j] = Math.Min(
                        Math.Min(
                            d[i - 1, j] + 1,      // Deletion
                            d[i, j - 1] + 1       // Insertion
                        ),
                        d[i - 1, j - 1] + cost    // Substitution
                    );
                }
            }

            return d[n, m];
        }

        public string CorrectNamesInQuery(string query, List<string> actorNames, List<string> directorNames)
        {
            if (string.IsNullOrWhiteSpace(query))
                return query;

            string correctedQuery = query;
            var allNames = new List<string>();

            if (actorNames != null)
                allNames.AddRange(actorNames);

            if (directorNames != null)
                allNames.AddRange(directorNames);

            if (!allNames.Any())
                return query;

            // Split query into words
            var words = query.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var correctedWords = new List<string>();

            foreach (var word in words)
            {
                // Check if this word (or combination with next word) matches a name
                string correctedWord = word;

                // Try matching single word
                correctedWord = FindClosestMatch(word, allNames, maxDistance: 2);

                // If single word didn't match well, try two-word combinations (for "leonardo dicaprio")
                if (correctedWord == word && words.Length > 1)
                {
                    int currentIndex = Array.IndexOf(words, word);
                    if (currentIndex < words.Length - 1)
                    {
                        string twoWord = $"{word} {words[currentIndex + 1]}";
                        string twoWordMatch = FindClosestMatch(twoWord, allNames, maxDistance: 3);

                        if (twoWordMatch != twoWord)
                        {
                            correctedWord = twoWordMatch;
                            // Skip next word since we matched two words
                            if (currentIndex + 1 < words.Length)
                                words[currentIndex + 1] = ""; // Mark as processed
                        }
                    }
                }

                correctedWords.Add(correctedWord);
            }

            // Reconstruct query, removing empty words
            correctedQuery = string.Join(" ", correctedWords.Where(w => !string.IsNullOrWhiteSpace(w)));

            return correctedQuery;
        }

        public string FindClosestMatch(string query, List<string> candidates, int maxDistance = 2)
        {
            if (string.IsNullOrWhiteSpace(query) || candidates == null || !candidates.Any())
                return query;

            // Normalize query (lowercase, trim)
            string normalizedQuery = query.ToLowerInvariant().Trim();

            // Find best match
            var bestMatch = candidates
                .Select(candidate => new
                {
                    Candidate = candidate,
                    Distance = CalculateEditDistance(normalizedQuery, candidate.ToLowerInvariant().Trim())
                })
                .Where(x => x.Distance <= maxDistance)
                .OrderBy(x => x.Distance)
                .ThenBy(x => Math.Abs(x.Candidate.Length - normalizedQuery.Length)) // Prefer similar length
                .FirstOrDefault();

            if (bestMatch != null)
            {
                return bestMatch.Candidate;
            }

            // No good match found, return original
            return query;
        }
    }
}
