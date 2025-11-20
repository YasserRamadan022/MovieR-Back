using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Ports
{
    public interface IQueryExpander
    {
        /// <summary>
        /// Expands a query by adding synonyms and related terms
        /// </summary>
        /// <param name="query">The search query to expand</param>
        /// <returns>List of expanded terms (original + synonyms)</returns>
        List<string> ExpandQuery(string query);

        /// <summary>
        /// Expands a single word by looking up synonyms
        /// </summary>
        /// <param name="word">The word to expand</param>
        /// <returns>List of synonyms including the original word</returns>
        List<string> ExpandWord(string word);

        /// <summary>
        /// Gets all expanded terms for a query (for BM25 search)
        /// </summary>
        /// <param name="query">The search query</param>
        /// <returns>HashSet of all unique expanded terms</returns>
        HashSet<string> GetExpandedTerms(string query);
    }
}
