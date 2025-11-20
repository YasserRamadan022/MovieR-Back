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
    public class QueryExpander : IQueryExpander
    {
        private readonly Dictionary<string, List<string>> _synonyms;
        private readonly Dictionary<string, List<string>> _genreMappings;
        public QueryExpander(ILogger<QueryExpander> logger)
        {
            _synonyms = InitializeSynonyms();
            _genreMappings = InitializeGenreMappings();
        }
        private Dictionary<string, List<string>> InitializeSynonyms()
        {
            return new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
        {
            { "funny", new List<string> { "funny", "comedy", "humorous", "hilarious", "comic", "witty" } },
            { "scary", new List<string> { "scary", "horror", "frightening", "terrifying", "spooky", "creepy", "chilling" } },
            { "sad", new List<string> { "sad", "emotional", "tragic", "dramatic", "melancholic", "depressing", "heartbreaking" } },
            { "happy", new List<string> { "happy", "joyful", "cheerful", "uplifting", "positive", "lighthearted" } },
            { "romantic", new List<string> { "romantic", "love", "romance", "heartwarming", "sweet", "charming" } },
            { "exciting", new List<string> { "exciting", "thrilling", "action-packed", "intense", "adrenaline", "fast-paced" } },
            { "boring", new List<string> { "boring", "slow", "dull", "tedious", "monotonous" } },
            
            { "space", new List<string> { "space", "sci-fi", "science fiction", "aliens", "outer space", "galaxy", "futuristic" } },
            { "superhero", new List<string> { "superhero", "super hero", "comic book", "marvel", "dc", "superpowers" } },
            { "zombie", new List<string> { "zombie", "undead", "apocalypse", "survival", "post-apocalyptic" } },
            { "vampire", new List<string> { "vampire", "vampires", "undead", "gothic", "supernatural" } },
            { "war", new List<string> { "war", "military", "battle", "combat", "soldier", "wartime" } },
            { "western", new List<string> { "western", "cowboy", "frontier", "wild west", "outlaw" } },
            { "martial", new List<string> { "martial", "kung fu", "karate", "fighting", "combat", "action" } },
            
            { "movie", new List<string> { "movie", "film", "picture", "cinema", "motion picture", "flick" } },
            { "film", new List<string> { "film", "movie", "picture", "cinema", "motion picture" } },
            { "story", new List<string> { "story", "tale", "narrative", "plot", "account" } },
            { "character", new List<string> { "character", "person", "role", "protagonist", "hero" } },
            
            { "action", new List<string> { "action", "adventure", "thriller", "exciting", "intense" } },
            { "adventure", new List<string> { "adventure", "action", "journey", "quest", "expedition" } },
            { "thriller", new List<string> { "thriller", "suspense", "mystery", "tension", "edge-of-seat" } },
            
            { "drama", new List<string> { "drama", "dramatic", "serious", "emotional", "intense" } },
            { "comedy", new List<string> { "comedy", "funny", "humorous", "comic", "lighthearted" } },
            { "horror", new List<string> { "horror", "scary", "frightening", "terrifying", "spooky" } },
            
            { "great", new List<string> { "great", "excellent", "amazing", "outstanding", "brilliant" } },
            { "good", new List<string> { "good", "nice", "decent", "solid", "fine" } },
            { "bad", new List<string> { "bad", "terrible", "awful", "poor", "horrible" } },
            { "classic", new List<string> { "classic", "timeless", "iconic", "legendary", "famous" } },
            { "new", new List<string> { "new", "recent", "latest", "modern", "contemporary" } },
            { "old", new List<string> { "old", "vintage", "classic", "retro", "dated" } }
        };
        }
        private Dictionary<string, List<string>> InitializeGenreMappings()
        {
            return new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase)
        {
            { "sci-fi", new List<string> { "sci-fi", "science fiction", "space", "futuristic", "aliens" } },
            { "science fiction", new List<string> { "science fiction", "sci-fi", "space", "futuristic" } },
            { "fantasy", new List<string> { "fantasy", "magic", "magical", "mythical", "enchanted" } },
            { "crime", new List<string> { "crime", "criminal", "gangster", "mafia", "heist" } },
            { "mystery", new List<string> { "mystery", "detective", "investigation", "puzzle", "whodunit" } }
        };
        }
        public List<string> ExpandQuery(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return new List<string>();

            var words = query.Split(new[] { ' ', '\t', '\n', '\r', ',', '.', '!', '?' },
                StringSplitOptions.RemoveEmptyEntries)
                .Where(w => w.Length > 1)
                .ToList();

            if (!words.Any())
                return new List<string> { query.Trim() };

            var expandedTerms = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var word in words)
            {
                var expanded = ExpandWord(word);
                foreach (var term in expanded)
                {
                    expandedTerms.Add(term.ToLowerInvariant());
                }
            }

            expandedTerms.Add(query.Trim().ToLowerInvariant());
            var result = expandedTerms.ToList();

            return result;
        }
        public List<string> ExpandWord(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
                return new List<string> { word ?? string.Empty };

            string normalizedWord = word.Trim().ToLowerInvariant();

            if (_synonyms.TryGetValue(normalizedWord, out var synonyms))
            {
                return synonyms;
            }

            if (_genreMappings.TryGetValue(normalizedWord, out var genreTerms))
            {
                return genreTerms;
            }

            return new List<string> { word };
        }
        public HashSet<string> GetExpandedTerms(string query)
        {
            var expanded = ExpandQuery(query);
            return new HashSet<string>(expanded, StringComparer.OrdinalIgnoreCase);
        }
    }
}
