using Application.DTOs;
using Application.Interfaces;
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
        public async Task<IActionResult> ToggleMovieVote(MovieVoteDTO movieVoteDTO)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var result = await _movieUseCase.VoteAsync(userId, movieVoteDTO);
            return StatusCode(result.StatusCode, result);
        }
    }
}
