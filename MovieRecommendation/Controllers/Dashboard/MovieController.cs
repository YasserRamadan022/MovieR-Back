using Application.DTOs;
using Application.Interfaces;
using Application.UseCases;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MovieRecommendation.Controllers.Dashboard
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
        [HttpPost("AddMovie")]
        public async Task<IActionResult> AddMovie([FromBody] AddMovieDTO movieDTO)
        {
            var result = await _movieUseCase.AddMovie(movieDTO);
            return StatusCode(result.StatusCode, result);
        }
    }
}
