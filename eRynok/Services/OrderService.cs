using eRynok.DTOs.Orders;
using eRynok.Models;
using eRynok.Repositories.Interfaces;
using eRynok.Services.Interfaces;

namespace eRynok.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;

        public OrderService(
            IOrderRepository orderRepository,
            IProductRepository productRepository)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
        }

        public async Task<IEnumerable<OrderResponseDto>> GetOrders(
            int? buyerId,
            int? sellerId,
            OrderStatus? status)
        {
            var orders = await _orderRepository.GetOrders(
                buyerId,
                sellerId,
                status);

            return orders.Select(MapToDto).ToList();
        }

        public async Task<OrderResponseDto?> GetOrder(int id)
        {
            var order = await _orderRepository.GetOrder(id);

            return order == null ? null : MapToDto(order);
        }

        public async Task<ServiceResult<OrderResponseDto>> CreateOrder(
            CreateOrderDto dto)
        {
            bool buyerExists = await _orderRepository.BuyerExists(
                dto.BuyerId);

            if (!buyerExists)
            {
                return ServiceResult<OrderResponseDto>.Failure(
                    ServiceError.InvalidReference,
                    "The specified buyer does not exist.");
            }

            var product = await _productRepository.GetProduct(dto.ProductId);

            if (product == null)
            {
                return ServiceResult<OrderResponseDto>.Failure(
                    ServiceError.InvalidReference,
                    "The specified product does not exist.");
            }

            if (dto.Quantity < 1 || dto.Quantity > 1000)
            {
                return ServiceResult<OrderResponseDto>.Failure(
                    ServiceError.InvalidReference,
                    "Quantity must be between 1 and 1000.");
            }

            var order = new Order
            {
                BuyerId = dto.BuyerId,
                ProductId = dto.ProductId,
                Quantity = dto.Quantity,
                UnitPrice = product.Price,
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                Product = product
            };

            await _orderRepository.AddOrder(order);

            return ServiceResult<OrderResponseDto>.Success(
                MapToDto(order));
        }

        public async Task<ServiceResult<bool>> UpdateOrderStatus(
            int id,
            UpdateOrderStatusDto dto)
        {
            var order = await _orderRepository.GetOrder(id);

            if (order == null)
            {
                return ServiceResult<bool>.Failure(
                    ServiceError.NotFound,
                    $"Order with ID {id} was not found.");
            }

            if (!Enum.IsDefined(typeof(OrderStatus), dto.Status))
            {
                return ServiceResult<bool>.Failure(
                    ServiceError.InvalidReference,
                    "The specified order status is invalid.");
            }

            order.Status = dto.Status;

            await _orderRepository.UpdateOrder(order);

            return ServiceResult<bool>.Success(true);
        }

        public async Task<ServiceResult<bool>> DeleteOrder(int id)
        {
            var order = await _orderRepository.GetOrder(id);

            if (order == null)
            {
                return ServiceResult<bool>.Failure(
                    ServiceError.NotFound,
                    $"Order with ID {id} was not found.");
            }

            if (order.Status != OrderStatus.Pending)
            {
                return ServiceResult<bool>.Failure(
                    ServiceError.Conflict,
                    "Only pending orders can be deleted.");
            }

            await _orderRepository.DeleteOrder(order);

            return ServiceResult<bool>.Success(true);
        }

        private static OrderResponseDto MapToDto(Order order)
        {
            return new OrderResponseDto
            {
                Id = order.Id,
                BuyerId = order.BuyerId,
                ProductId = order.ProductId,
                ProductName = order.Product?.Name ?? "Unknown product",
                Quantity = order.Quantity,
                UnitPrice = order.UnitPrice,
                TotalPrice = order.Quantity * order.UnitPrice,
                Status = order.Status,
                CreatedAt = order.CreatedAt
            };
        }
    }
}
