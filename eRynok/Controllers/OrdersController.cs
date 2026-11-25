using eRynok.DTOs.Orders;
using eRynok.DTOs.OrderGroups;
using eRynok.Models;
using eRynok.Services;
using eRynok.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace eRynok.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>
        /// Отримує список замовлень із можливістю фільтрації за покупцем, продавцем та статусом.
        /// </summary>
        /// <param name="buyerId">Ідентифікатор покупця для фільтрації.</param>
        /// <param name="sellerId">Ідентифікатор продавця для фільтрації.</param>
        /// <param name="status">Статус замовлень для фільтрації.</param>
        /// <response code="200">Список замовлень успішно отримано.</response>
        [HttpGet]
        [Authorize(Roles = "Buyer,Seller,Admin")]
        [ProducesResponseType(typeof(IEnumerable<OrderResponseDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetOrders(
            [FromQuery] int? buyerId,
            [FromQuery] int? sellerId,
            [FromQuery] OrderStatus? status)
        {
            var orders = await _orderService.GetOrders(
                buyerId,
                sellerId,
                status,
                GetCurrentUserId(),
                GetCurrentUserRole());

            return Ok(orders);
        }

        /// <summary>
        /// Отримує інформацію про замовлення за його ідентифікатором.
        /// </summary>
        /// <param name="id">Ідентифікатор замовлення.</param>
        /// <response code="200">Замовлення успішно знайдено.</response>
        /// <response code="403">Немає дозволу на перегляд цього замовлення.</response>
        /// <response code="404">Замовлення з таким ідентифікатором не знайдено.</response>
        [HttpGet("{id}")]
        [Authorize(Roles = "Buyer,Seller,Admin")]
        [ProducesResponseType(typeof(OrderResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetOrder(int id)
        {
            var result = await _orderService.GetOrder(
                id,
                GetCurrentUserId(),
                GetCurrentUserRole());

            if (!result.IsSuccess)
                return HandleFailure(result);

            return Ok(result.Data);
        }

        /// <summary>
        /// Отримує кошик поточного покупця.
        /// </summary>
        /// <response code="200">Кошик успішно отримано.</response>
        /// <response code="404">Кошик не знайдено.</response>
        [HttpGet("cart")]
        [Authorize(Roles = "Buyer")]
        [ProducesResponseType(typeof(OrderGroupResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCart()
        {
            var result = await _orderService.GetCart(
                GetCurrentUserId());

            if (!result.IsSuccess)
                return HandleFailure(result);

            return Ok(result.Data);
        }

        /// <summary>
        /// Створює групу замовлень, яка містить одне або декілька окремих замовлень.
        /// </summary>
        /// <param name="dto">Дані для створення групи замовлень.</param>
        /// <response code="201">Групу замовлень успішно створено.</response>
        /// <response code="400">Передані некоректні дані або один із товарів не існує.</response>
        [HttpPost]
        [Authorize(Roles = "Buyer")]
        [ProducesResponseType(typeof(OrderGroupResponseDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateOrder(CreateOrderDto dto)
        {
            var result = await _orderService.CreateOrder(
                dto,
                GetCurrentUserId());

            if (!result.IsSuccess)
                return HandleFailure(result);

            return StatusCode(
                StatusCodes.Status201Created,
                result.Data);
        }

        /// <summary>
        /// Змінює статус існуючого замовлення.
        /// Продавець може змінювати статус лише замовлень на власні товари.
        /// </summary>
        /// <param name="id">Ідентифікатор замовлення.</param>
        /// <param name="dto">Новий статус замовлення.</param>
        /// <response code="204">Статус замовлення успішно оновлено.</response>
        /// <response code="400">Переданий некоректний статус.</response>
        /// <response code="403">Немає дозволу на зміну цього замовлення.</response>
        /// <response code="404">Замовлення не знайдено.</response>
        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Seller,Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateOrderStatus(
            int id,
            UpdateOrderStatusDto dto)
        {
            var result = await _orderService.UpdateOrderStatus(
                id,
                dto,
                GetCurrentUserId(),
                GetCurrentUserRole());

            if (!result.IsSuccess)
                return HandleFailure(result);

            return NoContent();
        }

        /// <summary>
        /// Видаляє замовлення, якщо воно має статус Pending.
        /// Операція доступна лише адміністратору.
        /// </summary>
        /// <param name="id">Ідентифікатор замовлення.</param>
        /// <response code="204">Замовлення успішно видалено.</response>
        /// <response code="404">Замовлення не знайдено.</response>
        /// <response code="409">Неможливо видалити замовлення, оскільки його статус відмінний від Pending.</response>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Buyer,Admin")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            var result = await _orderService.DeleteOrder(
                id,
                GetCurrentUserId(),
                GetCurrentUserRole());

            if (!result.IsSuccess)
                return HandleFailure(result);

            return NoContent();
        }

        private int GetCurrentUserId()
        {
            return int.Parse(
                User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        private UserRole GetCurrentUserRole()
        {
            return Enum.Parse<UserRole>(
                User.FindFirstValue(ClaimTypes.Role)!,
                true);
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
                    StatusCode(403, new { message = result.Message }),

                _ => BadRequest(new { message = result.Message })
            };
        }
    }
}