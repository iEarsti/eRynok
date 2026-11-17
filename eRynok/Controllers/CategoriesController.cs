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

        [HttpGet]
        public IActionResult GetCategories([FromQuery] string? search)
        {
            var categories = _categoryService.GetCategories(search);

            return Ok(categories);
        }

        [HttpGet("{id}")]
        public IActionResult GetCategory(int id)
        {
            var category = _categoryService.GetCategory(id);

            if (category == null)
                return NotFound(new { message = $"Category with ID {id} was not found." });

            return Ok(category);
        }

        [HttpPost]
        public IActionResult CreateCategory(CreateCategoryDto dto)
        {
            var result = _categoryService.CreateCategory(dto);

            if (!result.IsSuccess)
                return HandleFailure(result);

            var category = result.Data!;

            return CreatedAtAction(
                nameof(GetCategory),
                new { id = category.Id },
                category);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateCategory(int id, UpdateCategoryDto dto)
        {
            var result = _categoryService.UpdateCategory(id, dto);

            if (!result.IsSuccess)
                return HandleFailure(result);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteCategory(int id)
        {
            var result = _categoryService.DeleteCategory(id);

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
