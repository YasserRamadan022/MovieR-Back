using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MovieRecommendation.Controllers.Dashboard
{
    [Route("api/[controller]")]
    [ApiController]
    public class GenreController : ControllerBase
    {
        private readonly IGenreUseCase _genreUseCase;
        public GenreController(IGenreUseCase genreUseCase)
        {
            _genreUseCase = genreUseCase ?? throw new ArgumentNullException(nameof(genreUseCase));
        }
        [HttpPost("AddGenre")]
        public async Task<IActionResult> AddGenre(AddGenreDTO genreDTO)
        {
            var result = await _genreUseCase.AddGenre(genreDTO);
            return StatusCode(result.StatusCode, result);
        }
    }
}
