using eRynok.DTOs.Users;
using eRynok.Models;
using eRynok.Services;
using eRynok.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace eRynok.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        /// <summary>
        /// Отримує список користувачів із можливістю пошуку та фільтрації за роллю.
        /// </summary>
        /// <param name="search">Пошуковий запит за ім'ям або електронною поштою.</param>
        /// <param name="role">Роль користувачів для фільтрації.</param>
        /// <response code="200">Список користувачів успішно отримано.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<UserResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUsers(
            [FromQuery] string? search,
            [FromQuery] UserRole? role)
        {
            var users = await _userService.GetUsers(search, role);

            return Ok(users);
        }

        /// <summary>
        /// Отримує інформацію про користувача за його ідентифікатором.
        /// </summary>
        /// <param name="id">Ідентифікатор користувача.</param>
        /// <response code="200">Користувача успішно знайдено.</response>
        /// <response code="404">Користувача з таким ідентифікатором не знайдено.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUser(int id)
        {
            var user = await _userService.GetUser(id);

            if (user == null)
                return NotFound(new { message = $"User with ID {id} was not found." });

            return Ok(user);
        }

        /// <summary>
        /// Створює нового користувача з відповідною роллю та профільними даними.
        /// </summary>
        /// <param name="dto">Дані для створення користувача.</param>
        /// <response code="201">Користувача успішно створено.</response>
        /// <response code="400">Передані некоректні дані або відсутні необхідні профільні дані.</response>
        /// <response code="409">Користувач із такою електронною поштою вже існує.</response>
        [HttpPost]
        [ProducesResponseType(typeof(UserResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateUser(CreateUserDto dto)
        {
            var result = await _userService.CreateUser(dto);

            if (!result.IsSuccess)
                return HandleFailure(result);

            var user = result.Data!;

            return CreatedAtAction(
                nameof(GetUser),
                new { id = user.Id },
                user);
        }

        /// <summary>
        /// Оновлює інформацію про користувача та його профіль.
        /// </summary>
        /// <param name="id">Ідентифікатор користувача.</param>
        /// <param name="dto">Нові дані користувача.</param>
        /// <response code="204">Інформацію про користувача успішно оновлено.</response>
        /// <response code="400">Передані некоректні дані або відсутні необхідні профільні дані.</response>
        /// <response code="404">Користувача не знайдено.</response>
        /// <response code="409">Електронна пошта вже використовується або неможливо змінити роль користувача через наявні товари.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateUser(int id, UpdateUserDto dto)
        {
            var result = await _userService.UpdateUser(id, dto);

            if (!result.IsSuccess)
                return HandleFailure(result);

            return NoContent();
        }

        /// <summary>
        /// Видаляє користувача, якщо він не має товарів або замовлень покупця.
        /// </summary>
        /// <param name="id">Ідентифікатор користувача.</param>
        /// <response code="204">Користувача успішно видалено.</response>
        /// <response code="404">Користувача не знайдено.</response>
        /// <response code="409">Неможливо видалити користувача, оскільки він має пов'язані товари або замовлення.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var result = await _userService.DeleteUser(id);

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
