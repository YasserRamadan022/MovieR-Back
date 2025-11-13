using Application.DTOs;
using Application.Interfaces;
using Application.UseCases;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

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
        [HttpGet("GetMoviesByGenre/{genreId}")]
        public async Task<IActionResult> GetMoviesByGenre(int genreId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _movieUseCase.GetMoviesByGenreAsync(genreId, pageNumber, pageSize);
            return StatusCode(result.StatusCode, result);
        }
    }
}
