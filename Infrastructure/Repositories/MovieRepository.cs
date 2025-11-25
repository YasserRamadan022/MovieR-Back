using Core.Domain.Common;
using Core.Domain.Common.RepositoryException;
using Core.Domain.Entities;
using Core.Domain.Enums;
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
    public class MovieRepository: GenericRepository<Movie>,IMovieRepository
    {
        private readonly AppDbContext _context;
        private readonly ILogger<MovieRepository> _logger;

        public MovieRepository(AppDbContext context, ILogger<MovieRepository> logger)
            : base(context, logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<PagedResult<Movie>> GetMoviesByGenreAsync(int genreId, int pageNumber = 1, int pageSize = 10)
        {
            if (genreId <= 0)
            {
                _logger.LogWarning("GetMoviesByGenreAsync called with invalid genre id: {GenreId}", genreId);
                throw new ArgumentNullException("Ivalid genre Id");
            }

            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 10;
            if (pageSize > 100) pageSize = 100;

            try
            {
                _logger.LogInformation("Getting movies by genre {GenreId}, Page: {PageNumber}, Size: {PageSize}",
                    genreId, pageNumber, pageSize);

                var totalCount = await _context.Movies
                    .Where(m => m.MovieGenres.Any(mg => mg.GenreId == genreId))
                    .CountAsync();

                var movies = await _context.Movies
                    .Where(m => m.MovieGenres.Any(mg => mg.GenreId == genreId))
                    .Include(m => m.Ratings)
                    .OrderByDescending(m => m.ReleaseYear)
                    .ThenBy(m => m.Title)
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                _logger.LogInformation("Found {Count} movies for genre {GenreId} (Page {PageNumber})",
                    movies.Count, genreId, pageNumber);

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
                throw new RepositoryException("Ivalid genre Id used to retrieve movies by genre", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while getting movies by genre {GenreId}", genreId);
                throw new RepositoryException("An unexpected error occurred while retrieving movies by genre", ex);
            }
        }

        public async Task<bool> RateAsync(string userId, int movieId, int rating)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("RateAsync called with invalid userId: {UserId}", userId);
                throw new RepositoryException("Invalid userId");
            }

            var movie = await _context.Movies.FindAsync(movieId);
            if (movie == null)
            {
                _logger.LogWarning("RateAsync called with invalid movieId: {MovieId}", movieId);
                throw new RepositoryException("Invalid movieId");
            }

            var existingRate = await _context.Ratings
                    .FirstOrDefaultAsync(v => v.UserId == userId && v.MovieId == movieId);

            if (existingRate == null)
            {
                var newRate = new Rating()
                {
                    UserId = userId,
                    MovieId = movieId,
                    RatingValue = rating,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Ratings.AddAsync(newRate);
                await _context.SaveChangesAsync();
                return true;
            }
            else if(existingRate.RatingValue != rating)
            {
                existingRate.RatingValue = rating;
                existingRate.UpdatedAt = DateTime.UtcNow;
                _context.Ratings.Update(existingRate);
                await _context.SaveChangesAsync();
                return true;
            }
            else
            {
                return true;
            }
        }
        public async Task<bool> RemoveRateAsync(string userId, int movieId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("RateAsync called with invalid userId: {UserId}", userId);
                throw new RepositoryException("Invalid userId");
            }

            var movie = await _context.Movies.FindAsync(movieId);
            if (movie == null)
            {
                _logger.LogWarning("RateAsync called with invalid movieId: {MovieId}", movieId);
                throw new RepositoryException("Invalid movieId");
            }

            var existingRate = await _context.Ratings
                    .FirstOrDefaultAsync(v => v.UserId == userId && v.MovieId == movieId);

            if (existingRate == null)
            {
                return true;
            }

            _context.Ratings.Remove(existingRate);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<bool> ToggleVoteAsync(string userId, int movieId, VoteType voteType)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("ToggleVoteAsync called with invalid userId: {UserId}", userId);
                throw new RepositoryException("Invalid userId");
            }

            var movie = await _context.Movies.FindAsync(movieId);
            if (movie == null)
            {
                _logger.LogWarning("ToggleVoteAsync called with invalid movieId: {MovieId}", movieId);
                throw new RepositoryException("Invalid movieId");
            }

            var existingVote = await _context.Votes
                    .FirstOrDefaultAsync(v => v.UserId == userId && v.MovieId == movieId);

            if (existingVote == null)
            {
                var newVote = new Vote()
                {
                    UserId = userId,
                    MovieId = movieId,
                    VoteType = voteType,
                    CreatedAt = DateTime.UtcNow
                };
                await _context.Votes.AddAsync(newVote);
                await _context.SaveChangesAsync();
                return true;
            }
            else if(existingVote.VoteType != voteType)
            {
                existingVote.VoteType = voteType;
                existingVote.CreatedAt = DateTime.UtcNow;
                _context.Votes.Update(existingVote);
                await _context.SaveChangesAsync();
                return true;
            }
            else
            {
                _context.Votes.Remove(existingVote);
                await _context.SaveChangesAsync();
                return true;
            }
        }
    }
}
