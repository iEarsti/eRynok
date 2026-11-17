using eRynok.DTOs.Users;
using eRynok.Models;
using eRynok.Services;
using eRynok.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace eRynok.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public IActionResult GetUsers(
            [FromQuery] string? search,
            [FromQuery] UserRole? role)
        {
            var users = _userService.GetUsers(search, role);

            return Ok(users);
        }

        [HttpGet("{id}")]
        public IActionResult GetUser(int id)
        {
            var user = _userService.GetUser(id);

            if (user == null)
                return NotFound(new { message = $"User with ID {id} was not found." });

            return Ok(user);
        }

        [HttpPost]
        public IActionResult CreateUser(CreateUserDto dto)
        {
            var result = _userService.CreateUser(dto);

            if (!result.IsSuccess)
                return HandleFailure(result);

            var user = result.Data!;

            return CreatedAtAction(
                nameof(GetUser),
                new { id = user.Id },
                user);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateUser(int id, UpdateUserDto dto)
        {
            var result = _userService.UpdateUser(id, dto);

            if (!result.IsSuccess)
                return HandleFailure(result);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteUser(int id)
        {
            var result = _userService.DeleteUser(id);

            if (!result.IsSuccess)
                return HandleFailure(result);

            return NoContent();
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

                _ => BadRequest(new { message = result.Message })
            };
        }
    }
}
