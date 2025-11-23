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
    public class GenreUseCase : IGenreUseCase
    {
        private readonly IGenericRepository<Genre> _genreRepository;
        private readonly ILogger<GenreUseCase> _logger;
        private readonly IMapper _mapper;
        public GenreUseCase(IGenericRepository<Genre> genreRepository, ILogger<GenreUseCase> logger, IMapper mapper)
        {
            _genreRepository = genreRepository ?? throw new ArgumentNullException(nameof(genreRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        public async Task<OpResult> AddGenre(AddGenreDTO genreDTO)
        {
            if(genreDTO == null)
            {
                _logger.LogWarning("AddGenre called with null AddGenreDTO");
                return new OpResult() { Success = false, Message = "Genre data cannot be null", StatusCode = 400 };
            }

            try
            {
                var newGenre = _mapper.Map<Genre>(genreDTO);
                var result = await _genreRepository.AddAsync(newGenre);

                return new OpResult() { Success = true, Message = "Genre added successfully", StatusCode = 201 };
            }
            catch (RepositoryException ex)
            {
                if (ex.Message.Contains("already exists"))
                {
                    return new OpResult() { Success = false, Message = "This genre already exists", StatusCode = 409 };
                }
                return new OpResult() { Success = false, Message = "Something went wrong", StatusCode = 500 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while adding genre");
                return new OpResult() { Success = false, Message = "Something went wrong", StatusCode = 500 };
            }
        }

        public async Task<OpResult> GetAll(int pageNumber = 1, int pageSize = 20)
        {
            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 20;
            try
            {
                var result = await _genreRepository.GetAll(pageNumber, pageSize);
                var genresList = _mapper.Map<List<GenresDTO>>(result.Data);

                var pagedResult = new PagedResult<GenresDTO>
                {
                    Data = genresList,
                    PageNumber = result.PageNumber,
                    PageSize = result.PageSize,
                    TotalCount = result.TotalCount
                };

                return new OpResult() { Success = true, Message = "Data retrieved successfully", StatusCode = 200, Data = pagedResult };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting genres");
                return new OpResult() { Success = false, Message = "Something went wrong", StatusCode = 500, Data = null };
            }
        }
    }
}
