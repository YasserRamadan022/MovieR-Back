using Core.Ports;
using Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Core.Domain.Common.RepositoryException;

namespace Infrastructure.Repositories
{
    public class GenericRepository<T>: IGenericRepository<T> where T : class
    {
        private readonly AppDbContext _context;
        private readonly ILogger<GenericRepository<T>> _logger;
        public GenericRepository(AppDbContext context, ILogger<GenericRepository<T>> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ICollection<T>> GetAll()
        {
            try
            {
                _logger.LogInformation("Attempting to get all data of entity of type {EntityType}", typeof(T).Name);
                var data = await _context.Set<T>().ToListAsync();
                _logger.LogInformation("Entity of type {EntityType} all data retrieved successfully", typeof(T).Name);
                return data;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while retrieving all data of entity of type {EntityType}", typeof(T).Name);
                throw new RepositoryException("An unexpected error occurred while retrieving all data of entity", ex);
            }
        }
        public async Task<T?> GetByIdAsync(int id)
        {
            if (id <= 0)
            {
                _logger.LogWarning("GetByIdAsync called with invalid id: {Id} for entity type {EntityType}", id, typeof(T).Name);
                return null;
            }
            try
            {
                var entity = await _context.Set<T>().FindAsync(id);

                if (entity == null)
                {
                    _logger.LogInformation("Entity of type {EntityType} with id {Id} not found", typeof(T).Name, id);
                }
                else
                {
                    _logger.LogInformation("Entity of type {EntityType} with id {Id} retrieved successfully", typeof(T).Name, id);
                }

                return entity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while retrieving entity of type {EntityType} with id {Id}", typeof(T).Name, id);
                throw new RepositoryException($"An unexpected error occurred while retrieving entity with id {id}", ex);
            }
        }
        public async Task<T> AddAsync(T entity)
        {
            if(entity == null)
            {
                _logger.LogWarning("Attempted to add null entity");
                throw new ArgumentNullException(nameof(entity), "Entity cannot be null");
            }
            try
            {
                _logger.LogInformation("Attempting to add entity of type {EntityType}", typeof(T).Name);
                await _context.Set<T>().AddAsync(entity);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Entity of type {EntityType} added successfully", typeof(T).Name);
                return entity;
            }
            catch (DbUpdateException ex)
            {
                if (IsUniqueConstraintViolation(ex))
                {
                    _logger.LogWarning(ex, "Unique constraint violation while adding entity of type {EntityType}", typeof(T).Name);
                    throw new RepositoryException("This entity already exists", ex);
                }
                if (IsForeignKeyViolation(ex))
                {
                    _logger.LogWarning(ex, "Foreign key constraint violation while adding entity of type {EntityType}", typeof(T).Name);
                    throw new RepositoryException("One or more referenced entities do not exist", ex);
                }
                _logger.LogError(ex, "Database update error while adding entity of type {EntityType}", typeof(T).Name);
                throw new RepositoryException("Failed to add entity.", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while adding entity of type {EntityType}", typeof(T).Name);
                throw new RepositoryException("An unexpected error occurred while adding entity", ex);
            }
        }
        public async Task<T> UpdateAsync(T entity)
        {
            if (entity == null)
            {
                _logger.LogWarning("Attempted to update null entity");
                throw new ArgumentNullException(nameof(entity), "Entity cannot be null");
            }
            try
            {
                _context.Set<T>().Update(entity);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Entity of type {EntityType} updated successfully", typeof(T).Name);
                return entity;
            }
            catch(DbUpdateConcurrencyException ex)
            {
                _logger.LogError(ex, "Concurrency conflict while updating entity of type {EntityType}. The entity was modified by another process.", typeof(T).Name);
                throw new RepositoryException("The entity was modified by another process. Please refresh and try again.", ex);
            }
            catch (DbUpdateException ex)
            {
                _logger.LogError(ex, "Database update error while updating entity of type {EntityType}", typeof(T).Name);
                throw new RepositoryException("Failed to update entity", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while updating entity of type {EntityType}", typeof(T).Name);
                throw new RepositoryException("An unexpected error occurred while updating entity", ex);
            }
        }
        public async Task<bool> DeleteAsync(T entity)
        {
            if(entity == null)
            {
                _logger.LogWarning("Attempted to delete null entity");
                throw new ArgumentNullException(nameof(entity), "Entity cannot be null");
            }
            try
            {
                _context.Set<T>().Remove(entity);
                await _context.SaveChangesAsync();
                _logger.LogInformation("Entity of type {EntityType} deleted successfully", typeof(T).Name);
                return true;
            }
            catch(DbUpdateException ex)
            {
                _logger.LogError(ex, "Database update error while deleting entity of type {EntityType}", typeof(T).Name);
                throw new RepositoryException("Failed to delete entity", ex);
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while deleting entity of type {EntityType}", typeof(T).Name);
                throw new RepositoryException("An unexpected error occurred while deleting entity", ex);
            }
        }
        private bool IsForeignKeyViolation(DbUpdateException ex)
        {
            var message = ex.InnerException?.Message ?? ex.Message;
            return message.Contains("FOREIGN KEY constraint")
                || (message.Contains("The INSERT statement conflicted") && message.Contains("FOREIGN KEY"))
                || (message.Contains("The DELETE statement conflicted") && message.Contains("FOREIGN KEY"));
        }

        private bool IsUniqueConstraintViolation(DbUpdateException ex)
        {
            var message = ex.InnerException?.Message ?? ex.Message;
            return message.Contains("UNIQUE KEY constraint")
                || message.Contains("UNIQUE constraint")
                || (message.Contains("The INSERT statement conflicted") && message.Contains("UNIQUE"))
                || (message.Contains("Cannot insert duplicate key") && message.Contains("UNIQUE"));
        }
    }
}
