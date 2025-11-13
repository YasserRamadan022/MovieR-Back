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
    public class MovieUseCase: IMovieUseCase
    {
        private readonly IGenericRepository<Movie> _movieRepository;
        private readonly ILogger<MovieUseCase> _logger;
        private readonly IMapper _mapper;
        public MovieUseCase(IGenericRepository<Movie> movieRepository, IMapper mapper, ILogger<MovieUseCase> logger)
        {
            _movieRepository = movieRepository ?? throw new ArgumentNullException(nameof(movieRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        }
        public async Task<OpResult> AddMovie(AddMovieDTO movieDTO)
        {
            if (movieDTO == null)
            {
                _logger.LogWarning("Attempted to use null movie dto");
                return new OpResult() { Success = false, Message = "Invalid request data", StatusCode = 400 };
            }

            try
            {
                var newMovie = _mapper.Map<Movie>(movieDTO);
                var result = await _movieRepository.AddAsync(newMovie);

                return new OpResult() { Success = true, Message = "Movie added successfully", StatusCode = 201 };
            }
            catch (RepositoryException ex)
            {
                if (ex.Message.Contains("already exists"))
                {
                    return new OpResult() { Success = false, Message = "This movie already exists", StatusCode = 409 };
                }
                if (ex.Message.Contains("referenced entities do not exist"))
                {
                    return new OpResult() { Success = false, Message = "One or more referenced entities do not exist", StatusCode = 400 };
                }
                return new OpResult() { Success = false, Message = "Something went wrong", StatusCode = 500 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred while adding movie");
                return new OpResult() { Success = false, Message = "Something went wrong", StatusCode = 500 };
            }
        }
    }
}
