using Microsoft.AspNetCore.Mvc;
using eRynok.Services;
using eRynok.Services.Interfaces;
using eRynok.DTOs.Categories;

namespace eRynok.Controllers
{
   [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        /// <summary>
        /// Отримує список усіх категорій.
        /// </summary>
        /// <param name="search">Пошуковий запит за назвою або описом категорії.</param>
        /// <response code="200">Список категорій успішно отримано.</response>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<CategoryResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCategories([FromQuery] string? search)
        {
            var categories = await _categoryService.GetCategories(search);

            return Ok(categories);
        }

        /// <summary>
        /// Отримує категорію за її ідентифікатором.
        /// </summary>
        /// <param name="id">Ідентифікатор категорії.</param>
        /// <response code="200">Категорію знайдено.</response>
        /// <response code="404">Категорію з таким ідентифікатором не знайдено.</response>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(CategoryResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCategory(int id)
        {
            var category = await _categoryService.GetCategory(id);

            if (category == null)
                return NotFound(new { message = $"Category with ID {id} was not found." });

            return Ok(category);
        }

        /// <summary>
        /// Створює нову категорію.
        /// </summary>
        /// <param name="dto">Дані для створення категорії.</param>
        /// <response code="201">Категорію успішно створено.</response>
        /// <response code="400">Передані некоректні дані.</response>
        /// <response code="409">Категорія з такою назвою вже існує.</response>
        [HttpPost]
        [ProducesResponseType(typeof(CategoryResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateCategory(CreateCategoryDto dto)
        {
            var result = await _categoryService.CreateCategory(dto);

            if (!result.IsSuccess)
                return HandleFailure(result);

            var category = result.Data!;

            return CreatedAtAction(
                nameof(GetCategory),
                new { id = category.Id },
                category);
        }

        /// <summary>
        /// Змінює існуючу категорію.
        /// </summary>
        /// <param name="id">Ідентифікатор категорії.</param>
        /// <response code="204">Категорію успішно змінено.</response>
        /// <response code="400">Передані некоректні дані.</response>
        /// <response code="409">Категорія з такою назвою вже існує.</response>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateCategory(int id, UpdateCategoryDto dto)
        {
            var result = await _categoryService.UpdateCategory(id, dto);

            if (!result.IsSuccess)
                return HandleFailure(result);

            return NoContent();
        }

        /// <summary>
        /// Змінює існуючу категорію.
        /// </summary>
        /// <param name="id">Ідентифікатор категорії.</param>
        /// <response code="204">Категорію успішно видалено.</response>
        /// <response code="404">Категорію не знайдено.</response>
        /// <response code="409">Не можна видалити категорію, яка має товари.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var result = await _categoryService.DeleteCategory(id);

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
