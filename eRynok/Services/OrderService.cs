using eRynok.Data;
using eRynok.DTOs.Orders;
using eRynok.Models;
using eRynok.Services.Interfaces;

namespace eRynok.Services
{
    public class OrderService : IOrderService
    {
        public IEnumerable<OrderResponseDto> GetOrders(
            int? buyerId,
            int? sellerId,
            OrderStatus? status)
        {
            var orders = eRynokData.Orders.AsEnumerable();

            if (buyerId.HasValue)
                orders = orders.Where(o => o.BuyerId == buyerId.Value);

            if (sellerId.HasValue)
            {
                var sellerProductIds = eRynokData.Products
                    .Where(p => p.UserId == sellerId.Value)
                    .Select(p => p.Id)
                    .ToList();

                orders = orders.Where(o => sellerProductIds.Contains(o.ProductId));
            }

            if (status.HasValue)
                orders = orders.Where(o => o.Status == status.Value);

            return orders
                .Select(MapToDto)
                .ToList();
        }

        public OrderResponseDto? GetOrder(int id)
        {
            var order = eRynokData.Orders
                .FirstOrDefault(o => o.Id == id);

            return order == null ? null : MapToDto(order);
        }

        public ServiceResult<OrderResponseDto> CreateOrder(CreateOrderDto dto)
        {
            var buyer = eRynokData.Users.FirstOrDefault(u =>
                u.Id == dto.BuyerId &&
                u.Role == UserRole.Buyer);

            if (buyer == null)
                return ServiceResult<OrderResponseDto>.Failure(
                    ServiceError.InvalidReference,
                    "The specified buyer does not exist.");

            var product = eRynokData.Products
                .FirstOrDefault(p => p.Id == dto.ProductId);

            if (product == null)
                return ServiceResult<OrderResponseDto>.Failure(
                    ServiceError.InvalidReference,
                    "The specified product does not exist.");

            if (dto.Quantity < 1 || dto.Quantity > 1000)
                return ServiceResult<OrderResponseDto>.Failure(
                    ServiceError.InvalidReference,
                    "Quantity must be between 1 and 1000.");

            var order = new Order
            {
                Id = eRynokData.Orders.Count > 0
                    ? eRynokData.Orders.Max(o => o.Id) + 1
                    : 1,
                BuyerId = dto.BuyerId,
                ProductId = dto.ProductId,
                Quantity = dto.Quantity,
                UnitPrice = product.Price,
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            eRynokData.Orders.Add(order);
            eRynokData.SaveOrders();

            return ServiceResult<OrderResponseDto>.Success(MapToDto(order));
        }

        public ServiceResult<bool> UpdateOrderStatus(
            int id,
            UpdateOrderStatusDto dto)
        {
            var order = eRynokData.Orders
                .FirstOrDefault(o => o.Id == id);

            if (order == null)
                return ServiceResult<bool>.Failure(
                    ServiceError.NotFound,
                    $"Order with ID {id} was not found.");

            if (!Enum.IsDefined(typeof(OrderStatus), dto.Status))
                return ServiceResult<bool>.Failure(
                    ServiceError.InvalidReference,
                    "The specified order status is invalid.");

            order.Status = dto.Status;

            eRynokData.SaveOrders();

            return ServiceResult<bool>.Success(true);
        }

        private static OrderResponseDto MapToDto(Order order)
        {
            var product = eRynokData.Products
                .FirstOrDefault(p => p.Id == order.ProductId);

            return new OrderResponseDto
            {
                Id = order.Id,
                BuyerId = order.BuyerId,
                ProductId = order.ProductId,
                ProductName = product?.Name ?? "Unknown product",
                Quantity = order.Quantity,
                UnitPrice = order.UnitPrice,
                TotalPrice = order.Quantity * order.UnitPrice,
                Status = order.Status,
                CreatedAt = order.CreatedAt
            };
        }
    }
}
