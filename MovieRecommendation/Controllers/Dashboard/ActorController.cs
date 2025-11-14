using Application.DTOs;
using Application.Interfaces;
using Application.UseCases;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MovieRecommendation.Controllers.Dashboard
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActorController : ControllerBase
    {
        private readonly IActorUseCase _actorUseCase;
        public ActorController(IActorUseCase actorUseCase)
        {
            _actorUseCase = actorUseCase ?? throw new ArgumentNullException(nameof(actorUseCase));
        }
        [HttpPost("AddActor")]
        public async Task<IActionResult> AddActor(AddActorDTO actorDTO)
        {
            var result = await _actorUseCase.AddActor(actorDTO);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet("ActorMovies/{actorId}")]
        public async Task<IActionResult> ActorMovies(int actorId, [FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var result = await _actorUseCase.GetActorMovies(actorId, pageNumber, pageSize);
            return StatusCode(result.StatusCode, result);
        }
    }
}
