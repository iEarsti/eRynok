using Microsoft.AspNetCore.Mvc;
using eRynok.DTOs.Products;
using eRynok.Services;
using eRynok.Services.Interfaces;

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
        [ProducesResponseType(typeof(ProductResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateProduct(CreateProductDto dto)
        {
            var result = await _productService.CreateProduct(dto);

            if (!result.IsSuccess)
                return HandleFailure(result);

            var product = result.Data!;

            return CreatedAtAction(
                nameof(GetProduct),
                new { id = product.Id },
                product);
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
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateProduct(int id, UpdateProductDto dto)
        {
            var result = await _productService.UpdateProduct(id, dto);

            if (!result.IsSuccess)
                return HandleFailure(result);

            return NoContent();
        }

        /// <summary>
        /// Видаляє товар, якщо він не пов'язаний з існуючими замовленнями.
        /// </summary>
        /// <param name="id">Ідентифікатор товару.</param>
        /// <response code="204">Товар успішно видалено.</response>
        /// <response code="404">Товар не знайдено.</response>
        /// <response code="409">Неможливо видалити товар, оскільки він присутній у замовленнях.</response>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var result = await _productService.DeleteProduct(id);

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
