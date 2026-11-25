using Microsoft.AspNetCore.Mvc;
using eRynok.DTOs.Products;
using eRynok.Services;
using eRynok.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace eRynok.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// Отримує список товарів із можливістю пошуку та фільтрації.
        /// </summary>
        /// <param name="search">Пошуковий запит за назвою або описом товару.</param>
        /// <param name="categoryId">Ідентифікатор категорії для фільтрації.</param>
        /// <param name="userId">Ідентифікатор продавця для фільтрації.</param>
        /// <param name="minPrice">Мінімальна ціна товару.</param>
        /// <param name="maxPrice">Максимальна ціна товару.</param>
        /// <response code="200">Список товарів успішно отримано.</response>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<ProductResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProducts(
            [FromQuery] string? search,
            [FromQuery] int? categoryId,
            [FromQuery] int? userId,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice)
        {
            var products = await _productService.GetProducts(
                search,
                categoryId,
                userId,
                minPrice,
                maxPrice);

            return Ok(products);
        }

        /// <summary>
        /// Отримує інформацію про товар за його ідентифікатором.
        /// </summary>
        /// <param name="id">Ідентифікатор товару.</param>
        /// <response code="200">Товар успішно знайдено.</response>
        /// <response code="404">Товар з таким ідентифікатором не знайдено.</response>
        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _productService.GetProduct(id);

            if (product == null)
                return NotFound(new { message = $"Product with ID {id} was not found." });

            return Ok(product);
        }

        /// <summary>
        /// Створює новий товар.
        /// </summary>
        /// <param name="dto">Дані для створення товару.</param>
        /// <response code="201">Товар успішно створено.</response>
        /// <response code="400">Передані некоректні дані або категорія чи продавець не існують.</response>
        [HttpPost]
        [Authorize(Roles = "Seller,Admin")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateProduct(
        [FromBody] CreateProductDto dto)
        {
            int currentUserId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            bool isAdmin = User.IsInRole("Admin");

            var result = await _productService.CreateProduct(
                dto, currentUserId, isAdmin);

            if (!result.IsSuccess)
                return HandleFailure(result);

            return CreatedAtAction(
                nameof(GetProduct),
                new { id = result.Data!.Id },
                result.Data);
        }

        /// <summary>
        /// Оновлює інформацію про існуючий товар.
        /// </summary>
        /// <param name="id">Ідентифікатор товару.</param>
        /// <param name="dto">Нові дані товару.</param>
        /// <response code="204">Товар успішно оновлено.</response>
        /// <response code="400">Передані некоректні дані або посилання на категорію чи продавця недійсне.</response>
        /// <response code="404">Товар не знайдено.</response>
        [HttpPut("{id}")]
        [Authorize(Roles = "Seller,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProduct(
        int id,
        [FromBody] UpdateProductDto dto)
        {
            int currentUserId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            bool isAdmin = User.IsInRole("Admin");

            var result = await _productService.UpdateProduct(
                id, dto, currentUserId, isAdmin);

            if (!result.IsSuccess)
                return HandleFailure(result);

            return Ok(new { message = "Товар успішно оновлено." });
        }

        /// <summary>
        /// Видаляє товар, якщо він не пов'язаний з існуючими замовленнями.
        /// </summary>
        /// <param name="id">Ідентифікатор товару.</param>
        /// <response code="204">Товар успішно видалено.</response>
        /// <response code="404">Товар не знайдено.</response>
        /// <response code="409">Неможливо видалити товар, оскільки він присутній у замовленнях.</response>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Seller,Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            int currentUserId = int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            bool isAdmin = User.IsInRole("Admin");

            var result = await _productService.DeleteProduct(
                id, currentUserId, isAdmin);

            if (!result.IsSuccess)
                return HandleFailure(result);

            return Ok(new { message = "Товар успішно видалено." });
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

                ServiceError.Forbidden =>
                    StatusCode(StatusCodes.Status403Forbidden, new { message = result.Message }),

                _ => BadRequest(new { message = result.Message })
            };
        }
    }
}
