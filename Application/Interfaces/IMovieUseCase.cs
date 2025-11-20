using Application.DTOs;
using Application.DTOs.Dashboard;
using Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IMovieUseCase
    {
        Task<OpResult> AddMovie(AddMovieDTO movie);
        Task<OpResult> GetMoviesByGenreAsync(int genreId, int pageNumber = 1, int pageSize = 10);
        Task<OpResult> VoteAsync(string userId, MovieVoteDTO voteDTO);
        Task<OpResult> RateAsync(string userId, MovieRateDTO rateDTO);
        Task<OpResult> RemoveRateAsync(string userId, int movieId);
        Task<OpResult> ForYou(string userId, int pageNumber = 1, int pageSize = 20);
    }
}
