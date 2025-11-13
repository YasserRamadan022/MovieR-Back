using Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.Ports
{
    public interface IUnitOfWork: IDisposable
    {
        IGenericRepository<Movie> Movies {get; }
        IGenericRepository<Comment> Comments {get; }
        IGenericRepository<Favorite> Favorites {get; }
        IGenericRepository<Interest> Interests {get; }
        IGenericRepository<Rating> Ratings {get; }
        IGenericRepository<Vote> Votes {get; }
        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
