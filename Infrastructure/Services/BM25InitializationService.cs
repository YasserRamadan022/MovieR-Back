using Core.Ports;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class BM25InitializationService: IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<BM25InitializationService> _logger;
        public BM25InitializationService(IServiceProvider serviceProvider, ILogger<BM25InitializationService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Starting BM25 initialization...");

            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var bm25Scorer = scope.ServiceProvider.GetRequiredService<IBM25Scorer>();

            try
            {
                // Load all movies with their descriptions
                var movies = await context.Movies
                    .AsNoTracking()
                    .Select(m => new
                    {
                        m.Id,
                        m.Title,
                        m.Description,
                        Genres = m.MovieGenres.Select(g => g.Genre.Name).ToList(),
                        Director = m.Director != null ? m.Director.Name : string.Empty,
                        Actors = m.MovieActors.Select(a => a.Actor.Name).ToList()
                    })
                    .ToListAsync(cancellationToken);

                // Combine all text fields for each movie
                var documents = movies.Select(m => new
                {
                    m.Id,
                    Text = CombineMovieFields(m.Title, m.Description, m.Genres, m.Director, m.Actors)
                }).ToList();

                // Initialize BM25 with all documents
                var documentList = documents.Select(d => (d.Id, d.Text)).ToList();
                await bm25Scorer.InitializeAsync(documentList);

                _logger.LogInformation("BM25 initialization completed. {Count} movies processed.", movies.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing BM25");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Combines all movie fields into a single searchable text
        /// </summary>
        private string CombineMovieFields(
            string title,
            string description,
            List<string> genres,
            string director,
            List<string> actors)
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(title))
                parts.Add(title);

            if (!string.IsNullOrWhiteSpace(description))
                parts.Add(description);

            if (genres != null && genres.Any())
                parts.AddRange(genres);

            if (!string.IsNullOrWhiteSpace(director))
                parts.Add(director);

            if (actors != null && actors.Any())
                parts.AddRange(actors);

            return string.Join(" ", parts);
        }
    }
}
