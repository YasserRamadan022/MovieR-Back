using Core.Domain.Common;
using Core.Domain.Entities;
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
    }
}
