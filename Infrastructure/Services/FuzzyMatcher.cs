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

            var words = query.Split(new[] { ' ', '\t', '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);
            var correctedWords = new List<string>();
            int processedIndex = -1;

            for (int i = 0; i < words.Length; i++)
            {
                if (i <= processedIndex)
                    continue;

                string correctedWord = words[i];

                correctedWord = FindClosestMatch(words[i], allNames);

                if (correctedWord == words[i] && i < words.Length - 1)
                {
                    string twoWord = $"{words[i]} {words[i + 1]}";
                    string twoWordMatch = FindClosestMatch(twoWord, allNames);

                    if (twoWordMatch != twoWord)
                    {
                        correctedWord = twoWordMatch;
                        processedIndex = i + 1;
                    }
                }

                correctedWords.Add(correctedWord);
            }

            correctedQuery = string.Join(" ", correctedWords.Where(w => !string.IsNullOrWhiteSpace(w)));

            return correctedQuery;
        }
        public string FindClosestMatch(string query, List<string> candidates, int maxDistance = 2)
        {
            if (string.IsNullOrWhiteSpace(query) || candidates == null || !candidates.Any())
                return query;

            string normalizedQuery = query.ToLowerInvariant().Trim();
            int queryLength = normalizedQuery.Length;

            int adaptiveMaxDistance = queryLength <= 6
                ? Math.Max(3, (int)(queryLength * 0.5))
                : Math.Max(4, (int)(queryLength * 0.3));

            var fullNameMatches = candidates
                .Select(candidate =>
                {
                    string candidateLower = candidate.ToLowerInvariant().Trim();
                    int distance = CalculateEditDistance(normalizedQuery, candidateLower);

                    bool isSubstringMatch = candidateLower.Contains(normalizedQuery) ||
                                           normalizedQuery.Contains(candidateLower);

                    if (isSubstringMatch && queryLength >= 3)
                    {
                        distance = 0;
                    }

                    return new
                    {
                        Candidate = candidate,
                        CandidateLower = candidateLower,
                        Distance = distance,
                        IsFullMatch = true,
                        IsSubstringMatch = isSubstringMatch
                    };
                })
                .Where(x => x.Distance <= adaptiveMaxDistance || x.IsSubstringMatch);

            var wordMatches = candidates
                .SelectMany(candidate =>
                {
                    var words = candidate.ToLowerInvariant().Trim()
                        .Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                    return words.Select(word =>
                    {
                        int distance = CalculateEditDistance(normalizedQuery, word);

                        bool isSubstringMatch = word.Contains(normalizedQuery) ||
                                               normalizedQuery.Contains(word);

                        if (isSubstringMatch && queryLength >= 3)
                        {
                            distance = 0;
                        }

                        return new
                        {
                            Candidate = candidate,
                            CandidateLower = word,
                            Distance = distance,
                            IsFullMatch = false,
                            IsSubstringMatch = isSubstringMatch
                        };
                    });
                })
                .Where(x => x.Distance <= adaptiveMaxDistance || x.IsSubstringMatch);

            var allMatches = fullNameMatches.Concat(wordMatches)
                .OrderBy(x => x.Distance)
                .ThenBy(x => x.IsSubstringMatch ? 0 : 1)
                .ThenBy(x => x.IsFullMatch ? 0 : 1)
                .ThenBy(x => Math.Abs(x.Candidate.Length - queryLength))
                .FirstOrDefault();

            if (allMatches != null)
            {
                return allMatches.Candidate;
            }

            return query;
        }

        public List<string> FindAllMatchingNames(string query, List<string> actorNames, List<string> directorNames)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<string>();

            var allNames = new List<string>();
            if (actorNames != null)
                allNames.AddRange(actorNames);
            if (directorNames != null)
                allNames.AddRange(directorNames);

            if (!allNames.Any())
                return new List<string>();

            string normalizedQuery = query.ToLowerInvariant().Trim();
            int queryLength = normalizedQuery.Length;

            int adaptiveMaxDistance = queryLength <= 6
                ? Math.Max(3, (int)(queryLength * 0.5))
                : Math.Max(4, (int)(queryLength * 0.3));

            var matchingNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var candidate in allNames)
            {
                string candidateLower = candidate.ToLowerInvariant().Trim();
                var words = candidateLower.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

                bool isMatch = false;

                // PRIORITIZE WORD-LEVEL MATCHES (more precise)
                foreach (var word in words)
                {
                    // 1. Exact word match (highest priority) - e.g., "leo" = "leo"
                    if (word.Equals(normalizedQuery, StringComparison.OrdinalIgnoreCase))
                    {
                        isMatch = true;
                        break;
                    }

                    // 2. Prefix match (e.g., "leo" matches "leonardo") - only if query is at least 3 chars
                    if (queryLength >= 3 && word.StartsWith(normalizedQuery, StringComparison.OrdinalIgnoreCase))
                    {
                        isMatch = true;
                        break;
                    }

                    // 3. Fuzzy match with edit distance (e.g., "lenard" matches "leonardo")
                    // Use stricter threshold for word-level fuzzy matching
                    int wordDistance = CalculateEditDistance(normalizedQuery, word);
                    int wordMaxDistance = Math.Min(adaptiveMaxDistance, Math.Max(2, (int)(word.Length * 0.3)));
                    
                    if (wordDistance <= wordMaxDistance && wordDistance < word.Length)
                    {
                        isMatch = true;
                        break;
                    }
                }

                // Only check full name match if no word match found (and use stricter criteria)
                if (!isMatch)
                {
                    // Full name match: only if query is a substring of the full name (not vice versa)
                    // This handles cases like "leonardo dicaprio" query matching "Leonardo DiCaprio"
                    if (candidateLower.Contains(normalizedQuery) && queryLength >= 3)
                    {
                        isMatch = true;
                    }
                    // Very close full name edit distance (stricter than word-level)
                    else
                    {
                        int fullNameDistance = CalculateEditDistance(normalizedQuery, candidateLower);
                        // Only match if edit distance is very small relative to query length
                        int strictMaxDistance = Math.Max(2, (int)(queryLength * 0.4));
                        if (fullNameDistance <= strictMaxDistance && fullNameDistance < candidateLower.Length / 2)
                        {
                            isMatch = true;
                        }
                    }
                }

                if (isMatch)
                {
                    matchingNames.Add(candidate);
                }
            }

            return matchingNames.ToList();
        }
    }
}
