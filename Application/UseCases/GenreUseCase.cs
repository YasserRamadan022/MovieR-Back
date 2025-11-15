using Application.DTOs;
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
    }
}
