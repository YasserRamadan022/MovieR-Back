using Application.DTOs;
using Application.Interfaces;
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
    }
}
