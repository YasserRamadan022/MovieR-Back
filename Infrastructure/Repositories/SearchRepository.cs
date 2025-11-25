using Core.Domain.Common;
using Core.Domain.Entities;
using Core.Ports;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class SearchRepository : ISearchRepository
    {
        private readonly AppDbContext _context;
        private readonly IFuzzyMatcher _fuzzyMatcher;
        private readonly IQueryExpander _queryExpander;
        private readonly IBM25Scorer _bm25Scorer;
        private readonly ILogger<SearchRepository> _logger;
        public SearchRepository(AppDbContext context, IFuzzyMatcher fuzzyMatcher, IQueryExpander queryExpander, IBM25Scorer bm25Scorer, ILogger<SearchRepository> logger)
        {
            _context = context;
            _fuzzyMatcher = fuzzyMatcher;
            _queryExpander = queryExpander;
            _bm25Scorer = bm25Scorer;
            _logger = logger;
        }

        private readonly Dictionary<string, double> _fieldWeights = new Dictionary<string, double>
        {
            { "Title", 3.0 },
            { "Description", 1.0 },
            { "Genres", 1.5 },
            { "Actors", 2.0 },
            { "Director", 2.0 }
        };
        public async Task<PagedResult<Movie>> SearchMoviesAsync(string query, int page, int pageSize)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return new PagedResult<Movie>() { Data = new List<Movie>(), PageNumber = page, PageSize = pageSize, TotalCount = 0 };
            }

            var actorNames = await _context.Actors
                .AsNoTracking()
                .Select(a => a.Name)
                .ToListAsync();

            var directorNames = await _context.Directors
                .AsNoTracking()
                .Select(d => d.Name)
                .ToListAsync();

            // Find all matching actor/director names (returns multiple matches)
            List<string> matchingNames = _fuzzyMatcher.FindAllMatchingNames(query, actorNames, directorNames);
            _logger.LogDebug($"Original query: '{query}' → Found {matchingNames.Count} matching names: {string.Join(", ", matchingNames)}");

            // Expand original query (preserves movie titles, genres, descriptions)
            List<string> expandedTerms = _queryExpander.ExpandQuery(query);
            
            // Add all matching names to expanded terms
            foreach (var name in matchingNames)
            {
                // Add full name as a term (for exact name matching)
                string fullNameLower = name.ToLowerInvariant();
                if (!expandedTerms.Contains(fullNameLower, StringComparer.OrdinalIgnoreCase))
                {
                    expandedTerms.Add(fullNameLower);
                }
                
                // Also add individual words (for partial matching)
                var nameWords = name.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)
                    .Where(w => w.Length > 2) // Only add words longer than 2 chars
                    .Select(w => w.ToLowerInvariant());
                
                foreach (var word in nameWords)
                {
                    if (!expandedTerms.Contains(word, StringComparer.OrdinalIgnoreCase))
                    {
                        expandedTerms.Add(word);
                    }
                }
            }
            
            _logger.LogDebug($"Expanded query terms: {string.Join(", ", expandedTerms)}");

            var movies = await _context.Movies
                .AsNoTracking()
                .Include(m => m.MovieGenres)
                    .ThenInclude(mg => mg.Genre)
                .Include(m => m.MovieActors)
                    .ThenInclude(ma => ma.Actor)
                .Include(m => m.Director)
                .ToListAsync();

            var scoredMovies = new List<(Movie Movie, double Score)>();

            foreach (var movie in movies)
            {
                var fields = new Dictionary<string, string>
                {
                    { "Title", movie.Title ?? "" },
                    { "Description", movie.Description ?? "" },
                    { "Genres", string.Join(" ", movie.MovieGenres.Select(g => g.Genre.Name)) },
                    { "Actors", string.Join(" ", movie.MovieActors.Select(a => a.Actor.Name)) },
                    { "Director", movie.Director?.Name ?? "" }
                };

                double score = _bm25Scorer.CalculateMultiFieldScore(
                    expandedTerms,
                    fields,
                    _fieldWeights,
                    movie.Id
                );

                if (score > 0)
                {
                    scoredMovies.Add((movie, score));
                }
            }
            scoredMovies = scoredMovies
                .OrderByDescending(x => x.Score)
                .ToList();

            int totalCount = scoredMovies.Count;
            double maxScore = scoredMovies.Any() ? scoredMovies[0].Score : 0;

            var paginatedMovies = scoredMovies
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(x => x.Movie)
                .ToList();

            _logger.LogInformation(
                $"Search completed: '{query}' → {totalCount} results, page {page}/{Math.Ceiling((double)totalCount / pageSize)}"
            );

            return new PagedResult<Movie>() { Data = paginatedMovies, PageNumber = page, PageSize = pageSize, TotalCount = totalCount };
        }
    }
}
