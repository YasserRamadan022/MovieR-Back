using Core.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface ISearchUseCase
    {
        Task<OpResult> SearchMoviesAsync(string query, int page = 1, int pageSize = 20);
    }
}
