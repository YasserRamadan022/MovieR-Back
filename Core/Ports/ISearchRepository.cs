using Core.Domain.Common;
using Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Ports
{
    public interface ISearchRepository
    {
        Task<PagedResult<Movie>> SearchMoviesAsync(string query, int page, int pageSize);
    }
}
