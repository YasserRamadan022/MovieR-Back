using Application.DTOs;
using Application.DTOs.Dashboard;
using Application.Interfaces;
using AutoMapper;
using Core.Domain.Common;
using Core.Domain.Common.RepositoryException;
using Core.Domain.Entities;
using Core.Ports;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.UseCases
{
    public class DirectorUseCase: IDirectorUseCase
    {
        private readonly IDirectorRepository _directorRepository;
        private readonly ILogger<DirectorUseCase> _logger;
        private readonly IMapper _mapper;

        public DirectorUseCase(IDirectorRepository directorRepository, ILogger<DirectorUseCase> logger, IMapper mapper)
        {
            _directorRepository = directorRepository ?? throw new ArgumentNullException(nameof(directorRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        public async Task<OpResult> GetDirectorMovies(int directorId, int pageNumber = 1, int pageSize = 20)
        {
            if (directorId <= 0)
            {
                _logger.LogWarning("GetActorMovies called with invalid director id: {DirectorId}", directorId);
                return new OpResult() { Success = false, Message = "Invalid director id", StatusCode = 400, Data = null };
            }

            try
            {
                var director = await _directorRepository.GetByIdAsync(directorId);
                if (director == null)
                {
                    return new OpResult() { Success = false, Message = "Invalid Director", StatusCode = 400, Data = null };
                }

                var directorData = _mapper.Map<DirectorDTO>(director);

                var result = await _directorRepository.GetDirectorMovies(directorId, pageNumber, pageSize);
                var moviesList = _mapper.Map<List<MoviesDTO>>(result.Data);

                var pagedResult = new PagedResult<MoviesDTO>
                {
                    Data = moviesList,
                    PageNumber = result.PageNumber,
                    PageSize = result.PageSize,
                    TotalCount = result.TotalCount
                };

                return new OpResult() { Success = true, Message = "Data retrieved successfully", StatusCode = 200, Data = new { directorData, pagedResult } };
            }
            catch (RepositoryException ex)
            {
                if (ex.Message.Contains("Ivalid director Id"))
                {
                    return new OpResult() { Success = false, Message = "Ivalid director Id", StatusCode = 400, Data = null };
                }
                _logger.LogError(ex, "Error getting movies by director {DirectorId}", directorId);
                return new OpResult() { Success = false, Message = "Something went wrong", StatusCode = 500, Data = null };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting movies by director {DirectorId}", directorId);
                return new OpResult() { Success = false, Message = "Something went wrong", StatusCode = 500, Data = null };
            }
        }
        public async Task<OpResult> AddDirector(AddDirectorDTO directorDTO)
        {
            if(directorDTO == null)
            {
                _logger.LogWarning("Attempted to use null director dto");
                return new OpResult() { Success = false, Message = "Invalid request data", StatusCode = 400 };
            }

            try
            {
                var newDirector = _mapper.Map<Director>(directorDTO);
                var result = await _directorRepository.AddAsync(newDirector);

                return new OpResult() { Success = true, Message = "Director added successfully", StatusCode = 201 };
            }
            catch (RepositoryException ex)
            {
                if (ex.Message.Contains("already exists"))
                {
                    return new OpResult() { Success = false, Message = "This director already exists", StatusCode = 409 };
                }
                return new OpResult() { Success = false, Message = "Something went wrong", StatusCode = 500 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while adding director");
                return new OpResult() { Success = false, Message = "Something went wrong", StatusCode = 500 };
            }
        }
        public async Task<OpResult> GetDirectors(int pageNumber = 1, int pageSize = 20)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 20;

            try
            {
                var result = await _directorRepository.GetAll(pageNumber, pageSize);
                var directorsList = _mapper.Map<List<DirectorDTO>>(result.Data);

                var pagedResult = new PagedResult<DirectorDTO>
                {
                    Data = directorsList,
                    PageNumber = result.PageNumber,
                    PageSize = result.PageSize,
                    TotalCount = result.TotalCount
                };

                return new OpResult() { Success = true, Message = "Data retrieved successfully", StatusCode = 200, Data = pagedResult };
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while retrieving directors");
                return new OpResult() { Success = false, Message = "Something went wrong", StatusCode = 500 };
            }
        }
    }
}
