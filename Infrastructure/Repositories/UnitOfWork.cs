using Core.Domain.Entities;
using Core.Ports;
using Infrastructure.Persistence;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;
        private readonly ILoggerFactory _loggerFactory;
        public IGenericRepository<Movie> Movies {get; }
        public IGenericRepository<Comment> Comments { get; }
        public IGenericRepository<Favorite> Favorites { get; }
        public IGenericRepository<Interest> Interests { get; }
        public IGenericRepository<Rating> Ratings { get; }
        public IGenericRepository<Vote> Votes { get; }
        public UnitOfWork(AppDbContext context, ILoggerFactory loggerFactory)
        {
            _context = context;
            _loggerFactory = loggerFactory;
            Movies = new GenericRepository<Movie>(_context, _loggerFactory.CreateLogger<GenericRepository<Movie>>());
            Comments = new GenericRepository<Comment>(_context, _loggerFactory.CreateLogger<GenericRepository<Comment>>());
            Favorites = new GenericRepository<Favorite>(_context, _loggerFactory.CreateLogger<GenericRepository<Favorite>>());
            Interests = new GenericRepository<Interest>(_context, _loggerFactory.CreateLogger<GenericRepository<Interest>>());
            Ratings = new GenericRepository<Rating>(_context, _loggerFactory.CreateLogger<GenericRepository<Rating>>());
            Votes = new GenericRepository<Vote>(_context, _loggerFactory.CreateLogger<GenericRepository<Vote>>());
        }
        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
        public Task BeginTransactionAsync()
        {
            throw new NotImplementedException();
        }
        public Task CommitTransactionAsync()
        {
            throw new NotImplementedException();
        }
        public Task RollbackTransactionAsync()
        {
            throw new NotImplementedException();
        }
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
