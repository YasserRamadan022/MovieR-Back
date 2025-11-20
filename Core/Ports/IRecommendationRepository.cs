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
        Task<List<(Movie Movie, double Score)>> GetSimilarMoviesByContentAsync(string userId);
        Task<List<(Movie Movie, double Score)>> GetMoviesBySimilarUsersAsync(string userId);
        Task<List<(Movie Movie, double Score)>> GetMoviesByMatrixFactorizationAsync(string userId);
        Task<List<(Movie Movie, double Score)>> GetPopularMoviesAsync();
        Task<PagedResult<Movie>> GetHybridRecommendationsAsync(string userId, int pageNumber = 1, int pageSize = 20);
    }
}
