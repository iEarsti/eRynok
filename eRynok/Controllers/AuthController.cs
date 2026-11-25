using eRynok.DTOs.Authorization;
using eRynok.Services;
using eRynok.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace eRynok.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Реєструє нового користувача.
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var result = await _authService.Register(dto);

            if (!result.IsSuccess)
                return HandleFailure(result);

            return Ok(result.Data);
        }

        /// <summary>
        /// Виконує авторизацію користувача та повертає JWT-токен.
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var result = await _authService.Login(dto);

            if (!result.IsSuccess)
                return HandleFailure(result);

            return Ok(result.Data);
        }

        /// <summary>
        /// Повертає інформацію про поточного авторизованого користувача.
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public IActionResult GetCurrentUser()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var name = User.FindFirstValue(ClaimTypes.Name);
            var email = User.FindFirstValue(ClaimTypes.Email);
            var role = User.FindFirstValue(ClaimTypes.Role);

            return Ok(new
            {
                Id = userId,
                Name = name,
                Email = email,
                Role = role
            });
        }

        private IActionResult HandleFailure<T>(ServiceResult<T> result)
        {
            return result.Error switch
            {
                ServiceError.NotFound =>
                    NotFound(new { message = result.Message }),

                ServiceError.InvalidReference =>
                    BadRequest(new { message = result.Message }),

                ServiceError.Conflict =>
                    Conflict(new { message = result.Message }),

                _ =>
                    BadRequest(new { message = result.Message })
            };
        }
    }
}