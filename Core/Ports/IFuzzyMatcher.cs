using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Ports
{
    public interface IFuzzyMatcher
    {
        /// <summary>
        /// Finds the closest match for a given query from a list of candidates
        /// </summary>
        /// <param name="query">The query to match (e.g., "lenardo")</param>
        /// <param name="candidates">List of candidate strings to match against</param>
        /// <param name="maxDistance">Maximum edit distance to accept (default: 2)</param>
        /// <returns>The closest match if found, otherwise returns the original query</returns>
        string FindClosestMatch(string query, List<string> candidates, int maxDistance = 2);

        /// <summary>
        /// Calculates the edit distance (Levenshtein distance) between two strings
        /// </summary>
        /// <param name="s1">First string</param>
        /// <param name="s2">Second string</param>
        /// <returns>Edit distance (0 = identical, higher = more different)</returns>
        int CalculateEditDistance(string s1, string s2);

        /// <summary>
        /// Corrects actor/director names in a query by fuzzy matching
        /// </summary>
        /// <param name="query">The search query</param>
        /// <param name="actorNames">List of all actor names in database</param>
        /// <param name="directorNames">List of all director names in database</param>
        /// <returns>Corrected query with proper names</returns>
        string CorrectNamesInQuery(string query, List<string> actorNames, List<string> directorNames);
    }
}
