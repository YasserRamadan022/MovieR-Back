using Core.Domain.Common;
using Core.Domain.Entities;
using Core.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Ports
{
    public interface IRecommendationRepository
    {
        // Get user preferences (ratings, votes, favorites, interests)
        Task<UserPreferences> GetUserPreferencesAsync(string userId);

        // Content-Based: Get movies similar to user's liked movies
        Task<List<Movie>> GetSimilarMoviesByContentAsync(UserPreferences preferences, int pageNumber, int pageSize);

        // Collaborative: Get movies liked by similar users
        Task<List<Movie>> GetMoviesBySimilarUsersAsync(string userId, int pageNumber, int pageSize);

        // Matrix Factorization: Get movies based on hidden factors
        Task<List<Movie>> GetMoviesByMatrixFactorizationAsync(string userId, int pageNumber, int pageSize);

        // Popularity: Get trending/popular movies
        Task<List<Movie>> GetPopularMoviesAsync(int pageNumber, int pageSize);

        // Hybrid: Get final recommendations (combines all 4)
        Task<PagedResult<Movie>> GetHybridRecommendationsAsync(string userId, int pageNumber = 1, int pageSize = 20);
    }
}
