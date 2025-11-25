using Application.DTOs;
using Application.Interfaces;
using Core.Domain.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace MovieRecommendation.Controllers.User
{
    [Route("api/[controller]")]
    [ApiController]
    public class MovieController : ControllerBase
    {
        private readonly IMovieUseCase _movieUseCase;
        private readonly ISearchUseCase _searchUseCase;
        public MovieController(IMovieUseCase movieUseCase, ISearchUseCase searchUseCase)
        {
            _movieUseCase = movieUseCase ?? throw new ArgumentNullException(nameof(movieUseCase));
            _searchUseCase = searchUseCase ?? throw new ArgumentNullException(nameof(searchUseCase));
        }
        [Authorize]
        [HttpPost("ToggleMovieVote")]
        public async Task<IActionResult> ToggleMovieVote([FromBody] MovieVoteDTO movieVoteDTO)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new OpResult { Success = false, Message = "User not authenticated", StatusCode = 401 });
            }

            var result = await _movieUseCase.VoteAsync(userId, movieVoteDTO);
            return StatusCode(result.StatusCode, result);
        }
        [Authorize]
        [HttpPost("RateMovie")]
        public async Task<IActionResult> RateMovie([FromBody] MovieRateDTO movieRateDTO)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new OpResult { Success = false, Message = "User not authenticated", StatusCode = 401 });
            }

            var result = await _movieUseCase.RateAsync(userId, movieRateDTO);
            return StatusCode(result.StatusCode, result);
        }
        [Authorize]
        [HttpDelete("DeleteRating/{movieId}")]
        public async Task<IActionResult> DeleteRating(int movieId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new OpResult { Success = false, Message = "User not authenticated", StatusCode = 401 });
            }

            var result = await _movieUseCase.RemoveRateAsync(userId, movieId);
            return StatusCode(result.StatusCode, result);
        }
        [Authorize]
        [HttpGet("ForYou/{pageNumber}/{pageSize}")]
        public async Task<IActionResult> ForYou(int pageNumber, int pageSize)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new OpResult { Success = false, Message = "User not authenticated", StatusCode = 401 });
            }

            var result = await _movieUseCase.ForYou(userId, pageNumber, pageSize);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet("Search/{query}")]
        public async Task<IActionResult> Search(string query, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            var result = await _searchUseCase.SearchMoviesAsync(query);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet("GetMovieDetails/{movieId}")]
        public async Task<IActionResult> GetMovieDetails(int movieId)
        {
            var result = await _movieUseCase.GetMovieDetailsAsync(movieId);
            return StatusCode(result.StatusCode, result);
        }
    }
}
