using eRynok.DTOs.Orders;
using eRynok.Models;
using eRynok.Services.Interfaces;
using eRynok.Services;
using Microsoft.AspNetCore.Mvc;

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

        [HttpGet]
        public IActionResult GetOrders(
            [FromQuery] int? buyerId,
            [FromQuery] int? sellerId,
            [FromQuery] OrderStatus? status)
        {
            var orders = _orderService.GetOrders(buyerId, sellerId, status);
            return Ok(orders);
        }

        [HttpGet("{id}")]
        public IActionResult GetOrder(int id)
        {
            var order = _orderService.GetOrder(id);

            if (order == null)
                return NotFound(new { message = $"Order with ID {id} was not found." });

            return Ok(order);
        }

        [HttpPost]
        public IActionResult CreateOrder(CreateOrderDto dto)
        {
            var result = _orderService.CreateOrder(dto);

            if (!result.IsSuccess)
                return HandleFailure(result);

            var order = result.Data!;

            return CreatedAtAction(
                nameof(GetOrder),
                new { id = order.Id },
                order);
        }

        [HttpPut("{id}/status")]
        public IActionResult UpdateOrderStatus(
            int id,
            UpdateOrderStatusDto dto)
        {
            var result = _orderService.UpdateOrderStatus(id, dto);

            if (!result.IsSuccess)
                return HandleFailure(result);

            return NoContent();
        }

        private IActionResult HandleFailure<T>(ServiceResult<T> result)
        {
            return result.Error switch
            {
                ServiceError.NotFound => NotFound(new { message = result.Message }),
                ServiceError.InvalidReference => BadRequest(new { message = result.Message }),
                ServiceError.Conflict => Conflict(new { message = result.Message }),
                _ => BadRequest(new { message = result.Message })
            };
        }
    }
}
