using Application.DTOs.Authentication;
using Application.Interfaces;
using Core.Domain.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace MovieRecommendation.Controllers.Authentication
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthenticationController : ControllerBase
    {
        private readonly IAuthUseCase _authUseCase;
        public AuthenticationController(IAuthUseCase authUseCase)
        {
            _authUseCase = authUseCase ?? throw new ArgumentNullException(nameof(authUseCase));
        }

        [HttpPost("SignUp")]
        public async Task<IActionResult> SignUp([FromBody] SignUpDTO signUpDTO)
        {
            var result = await _authUseCase.SignUpAsync(signUpDTO);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPost("SignIn")]
        public async Task<IActionResult> SignIn([FromBody] SignInDTO signInDTO)
        {
            var result = await _authUseCase.SignInAsync(signInDTO);
            return StatusCode(result.StatusCode, result);
        }
        [HttpGet("ConfirmEmail")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] string userId, [FromQuery] string token)
        {
            var result = await _authUseCase.ConfirmEmailAsync(userId, token);
            return StatusCode(result.StatusCode, result);
        }
        [HttpPost("ResendConfirmationEmail")]
        public async Task<IActionResult> ResendConfirmationEmail([FromBody] ResendConfirmationEmailDTO dto)
        {
            var result = await _authUseCase.ResendConfirmationEmailAsync(dto);
            return StatusCode(result.StatusCode, result);
        }
    }
}
