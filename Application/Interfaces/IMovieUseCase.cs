using Application.DTOs;
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
    }
}
