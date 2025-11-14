using Core.Domain.Common;
using Core.Domain.Common.RepositoryException;
using Core.Domain.Entities;
using Core.Ports;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class ActorRepository: GenericRepository<Actor>, IActorRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ActorRepository> _logger;
        public ActorRepository(AppDbContext context, ILogger<ActorRepository> logger)
            : base(context, logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PagedResult<Movie>> GetActorMovies(int actorId, int pageNumber = 1, int pageSize = 10)
        {
            if(actorId <= 0)
            {
                throw new ArgumentException("Invalid actor Id.");
            }

            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            try
            {
                _logger.LogInformation("Getting actor movies by actorId: {ActorId}, Page: {PageNumber}, Size: {PageSize}",
                        actorId, pageNumber, pageSize);

                var totalCount = await _context.Movies
                    .Where(m => m.MovieActors.Any(ma => ma.ActorId == actorId))
                    .CountAsync();

                var movies = await _context.Movies
                    .Where(m => m.MovieActors.Any(ma => ma.ActorId == actorId))
                    .OrderByDescending(m => m.ReleaseYear)
                    .ThenBy(m => m.Title)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                _logger.LogInformation("Found {Count} movies for actor {ActorId} (Page {PageNumber})",
                        movies.Count, actorId, pageNumber);

                return new PagedResult<Movie>
                {
                    Data = movies,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount
                };
            }
            catch (ArgumentNullException ex)
            {
                throw new RepositoryException("Invalid actor Id used to retrieve movies by actor", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while getting movies by actor {ActorId}", actorId);
                throw new RepositoryException("An unexpected error occurred while retrieving movies by actor", ex);
            }
        }
    }
}
