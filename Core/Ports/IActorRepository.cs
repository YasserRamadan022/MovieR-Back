using Core.Domain.Common;
using Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Ports
{
    public interface IActorRepository: IGenericRepository<Actor>
    {
        Task<PagedResult<Movie>> GetActorMovies(int actorId, int pageNumber = 1, int pageSize = 10);
    }
}
