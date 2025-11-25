using Application.DTOs;
using Application.DTOs.Dashboard;
using Application.Interfaces;
using AutoMapper;
using Core.Domain.Common;
using Core.Domain.Common.RepositoryException;
using Core.Domain.Entities;
using Core.Ports;
using Microsoft.AspNetCore.Mvc.RazorPages;
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
        private readonly IMovieRepository _movieRepository;
        private readonly IRecommendationRepository _recommendationRepository;
        private readonly ILogger<MovieUseCase> _logger;
        private readonly IMapper _mapper;
        public MovieUseCase(IMovieRepository movieRepository, IMapper mapper, ILogger<MovieUseCase> logger, IRecommendationRepository recommendationRepository)
        {
            _movieRepository = movieRepository ?? throw new ArgumentNullException(nameof(movieRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _recommendationRepository = recommendationRepository ?? throw new ArgumentNullException(nameof(recommendationRepository));
        }
        public async Task<OpResult> GetMoviesByGenreAsync(int genreId, int pageNumber = 1, int pageSize = 10)
        {
            if (genreId <= 0)
            {
                _logger.LogWarning("GetMoviesByGenreAsync called with invalid genre id: {GenreId}", genreId);
                return new OpResult() { Success = false, Message = "Invalid genre id", StatusCode = 400, Data = null };
            }

            try
            {
                var result = await _movieRepository.GetMoviesByGenreAsync(genreId, pageNumber, pageSize);
                var moviesList = _mapper.Map<List<MoviesDTO>>(result.Data);

                var pagedResult = new PagedResult<MoviesDTO>
                {
                    Data = moviesList,
                    PageNumber = result.PageNumber,
                    PageSize = result.PageSize,
                    TotalCount = result.TotalCount
                };
                
                return new OpResult() { Success = true, Message = "Data retrieved successfully", StatusCode = 200, Data = pagedResult };
            }
            catch(RepositoryException ex)
            {
                if(ex.Message.Contains("Ivalid genre Id"))
                {
                    return new OpResult() { Success = false, Message = "Ivalid genre Id", StatusCode = 400, Data = null };
                }
                _logger.LogError(ex, "Error getting movies by genre {GenreId}", genreId);
                return new OpResult() { Success = false, Message = "Something went wrong", StatusCode = 500, Data = null };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting movies by genre {GenreId}", genreId);
                return new OpResult() { Success = false, Message = "Something went wrong", StatusCode = 500, Data = null };
            }
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
        public async Task<OpResult> VoteAsync(string userId, MovieVoteDTO voteDTO)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User ID is null or empty.");
                    return new OpResult() { Success = false, Message = "User ID cannot be null or empty", StatusCode = 400 };
                }

                var result = await _movieRepository.ToggleVoteAsync(userId, voteDTO.MovieId, voteDTO.VoteType);
                return new OpResult() { Data = result, Success = true, Message = "Vote processed successfully", StatusCode = 200 };
            }
            catch (RepositoryException ex)
            {
                if (ex.Message.Contains("Invalid userId"))
                {
                    _logger.LogWarning("Invalid user ID provided.");
                    return new OpResult() { Success = false, Message = "Invalid user ID", StatusCode = 400 };
                }
                if (ex.Message.Contains("Invalid movieId"))
                {
                    _logger.LogWarning("Invalid movie ID provided.");
                    return new OpResult() { Success = false, Message = "Invalid movie ID", StatusCode = 400 };
                }
                _logger.LogError(ex, "An error occurred while processing the vote.");
                return new OpResult() { Success = false, Message = "An error occurred while processing the vote", StatusCode = 500 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the vote.");
                return new OpResult() { Success = false, Message = "An error occurred while processing the vote", StatusCode = 500 };
            }
        }
        public async Task<OpResult> RateAsync(string userId, MovieRateDTO rateDTO)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User ID is null or empty.");
                    return new OpResult() { Success = false, Message = "User ID cannot be null or empty", StatusCode = 400 };
                }

                var result = await _movieRepository.RateAsync(userId, rateDTO.MovieId, rateDTO.RatingValue);
                return new OpResult() { Data = result, Success = true, Message = "Rating processed successfully", StatusCode = 200 };
            }
            catch (RepositoryException ex)
            {
                if (ex.Message.Contains("Invalid userId"))
                {
                    _logger.LogWarning("Invalid user ID provided.");
                    return new OpResult() { Success = false, Message = "Invalid user ID", StatusCode = 400 };
                }
                if (ex.Message.Contains("Invalid movieId"))
                {
                    _logger.LogWarning("Invalid movie ID provided.");
                    return new OpResult() { Success = false, Message = "Invalid movie ID", StatusCode = 400 };
                }
                _logger.LogError(ex, "An error occurred while processing the rate.");
                return new OpResult() { Success = false, Message = "An error occurred while processing the rate", StatusCode = 500 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the rate.");
                return new OpResult() { Success = false, Message = "An error occurred while processing the rate", StatusCode = 500 };
            }
        }
        public async Task<OpResult> RemoveRateAsync(string userId, int movieId)
        {
            try
            {
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("User ID is null or empty.");
                    return new OpResult() { Success = false, Message = "User ID cannot be null or empty", StatusCode = 400 };
                }

                var result = await _movieRepository.RemoveRateAsync(userId, movieId);
                return new OpResult() { Data = result, Success = true, Message = "Rate deleted successfully", StatusCode = 200 };
            }
            catch (RepositoryException ex)
            {
                if (ex.Message.Contains("Invalid userId"))
                {
                    _logger.LogWarning("Invalid user ID provided.");
                    return new OpResult() { Success = false, Message = "Invalid user ID", StatusCode = 400 };
                }
                if (ex.Message.Contains("Invalid movieId"))
                {
                    _logger.LogWarning("Invalid movie ID provided.");
                    return new OpResult() { Success = false, Message = "Invalid movie ID", StatusCode = 400 };
                }
                _logger.LogError(ex, "An error occurred while deleting the rate.");
                return new OpResult() { Success = false, Message = "An error occurred while deleting the rate", StatusCode = 500 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the rate.");
                return new OpResult() { Success = false, Message = "An error occurred while deleting the rate", StatusCode = 500 };
            }
        }
        public async Task<OpResult> ForYou(string userId, int pageNumber = 1, int pageSize = 20)
        {
            if (userId == null)
            {
                _logger.LogWarning("ForYou called with null user id");
                return new OpResult() { Success = false, Message = "Invalid user id", StatusCode = 400, Data = null };
            }

            if (pageNumber < 1) pageNumber = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 20;
            try
            {
                var recommeddedMovies = await _recommendationRepository.GetHybridRecommendationsAsync(userId, pageNumber, pageSize);
                var moviesList = _mapper.Map<List<MoviesDTO>>(recommeddedMovies.Data);
                return new OpResult() { Success = true, Message = "Data retrieved successfully", StatusCode = 200, Data = moviesList };
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error getting movies for user {UserId}", userId);
                return new OpResult() { Success = false, Message = "Something went wrong", StatusCode = 500, Data = null };
            }
        }
        public async Task<OpResult> GetMovieDetailsAsync(int movieId)
        {
            if(movieId <= 0)
            {
                _logger.LogWarning("GetMovieDetailsAsync called with invalid movie id: {MovieId}", movieId);
                return new OpResult() { Success = false, Message = "Invalid movie id", StatusCode = 400, Data = null };
            }

            try
            {
                var result = await _movieRepository.GetByIdAsync(movieId);
                var movieDetails = _mapper.Map<MovieDetailsDTO>(result);

                return new OpResult() { Success = true, Message = "Data retrieved successfully", StatusCode = 200, Data = movieDetails };
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Error getting movie details for movie {MovieId}", movieId);
                return new OpResult() { Success = false, Message = "Something went wrong", StatusCode = 500, Data = null };
            }
        }
    }
}
