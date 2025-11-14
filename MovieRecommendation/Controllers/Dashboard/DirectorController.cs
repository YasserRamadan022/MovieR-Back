using Application.DTOs;
using Application.Interfaces;
using Application.UseCases;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MovieRecommendation.Controllers.Dashboard
{
    [Route("api/[controller]")]
    [ApiController]
    public class DirectorController : ControllerBase
    {
        private readonly IDirectorUseCase _directorUseCase;
        public DirectorController(IDirectorUseCase directorUseCase)
        {
            _directorUseCase = directorUseCase ?? throw new ArgumentNullException(nameof(directorUseCase));
        }
        [HttpPost("AddDirector")]
        public async Task<IActionResult> AddDirector([FromBody] AddDirectorDTO directorDTO)
        {
            var result = await _directorUseCase.AddDirector(directorDTO);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet("DirectorMovies/{directorId}")]
        public async Task<IActionResult> ActorMovies(int directorId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _directorUseCase.GetDirectorMovies(directorId, pageNumber, pageSize);
            return StatusCode(result.StatusCode, result);
        }
    }
}
