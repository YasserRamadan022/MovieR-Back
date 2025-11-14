using Core.Domain.Common;
using Core.Domain.Common.RepositoryException;
using Core.Domain.Entities;
using Core.Ports;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class DirectorRepository: GenericRepository<Director>, IDirectorRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DirectorRepository> _logger;
        public DirectorRepository(AppDbContext context, ILogger<DirectorRepository> logger) : base(context, logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<PagedResult<Movie>> GetDirectorMovies(int directorId, int pageNumber = 1, int pageSize = 10)
        {
            if (directorId <= 0)
            {
                throw new ArgumentException("Invalid director Id.");
            }

            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            try
            {
                _logger.LogInformation("Getting movies by directorId: {DirectorId}, Page: {PageNumber}, Size: {PageSize}",
                        directorId, pageNumber, pageSize);

                var totalCount = await _context.Movies
                    .CountAsync(m => m.DirectorId == directorId);

                var movies = await _context.Movies
                    .Where(m => m.DirectorId == directorId)
                    .OrderByDescending(m => m.ReleaseYear)
                    .ThenBy(m => m.Title)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                _logger.LogInformation("Found {Count} movies for director {DirectorId} (Page {PageNumber})",
                        movies.Count, directorId, pageNumber);

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
                throw new RepositoryException("Ivalid director Id used to retrieve movies by director", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while getting movies by director {DirectorId}", directorId);
                throw new RepositoryException("An unexpected error occurred while retrieving movies by director", ex);
            }
        }
    }
}
