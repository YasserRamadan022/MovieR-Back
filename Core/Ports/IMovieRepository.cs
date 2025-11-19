using Core.Domain.Common;
using Core.Domain.Entities;
using Core.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Ports
{
    public interface IMovieRepository: IGenericRepository<Movie>
    {
        Task<PagedResult<Movie>> GetMoviesByGenreAsync(int genreId, int pageNumber = 1, int pageSize = 10);
        Task<bool> ToggleVoteAsync(string userId, int movieId, VoteType voteType);
        Task<bool> RateAsync(string userId, int movieId, int rating);
        Task<bool> RemoveRateAsync(string userId, int movieId);
    }
}
