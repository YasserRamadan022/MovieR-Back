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
        public MovieController(IMovieUseCase movieUseCase)
        {
            _movieUseCase = movieUseCase ?? throw new ArgumentNullException(nameof(movieUseCase));
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
    }
}
