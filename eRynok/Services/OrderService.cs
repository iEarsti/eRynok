using eRynok.Data;
using eRynok.DTOs.Orders;
using eRynok.Models;
using eRynok.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace eRynok.Services
{
    public class OrderService : IOrderService
    {
        private readonly eRynokDbContext _context;

        public OrderService(eRynokDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<OrderResponseDto>> GetOrders(
            int? buyerId,
            int? sellerId,
            OrderStatus? status)
        {
            var query = _context.Orders
                .AsNoTracking()
                .Include(o => o.Product)
                .AsQueryable();

            if (buyerId.HasValue)
            {
                query = query.Where(o => o.BuyerId == buyerId.Value);
            }

            if (sellerId.HasValue)
            {
                query = query.Where(o => o.Product.UserId == sellerId.Value);
            }

            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }

            var orders = await query.ToListAsync();

            return orders.Select(MapToDto).ToList();
        }

        public async Task<OrderResponseDto?> GetOrder(int id)
        {
            var order = await _context.Orders
                .AsNoTracking()
                .Include(o => o.Product)
                .FirstOrDefaultAsync(o => o.Id == id);

            return order == null ? null : MapToDto(order);
        }

        public async Task<ServiceResult<OrderResponseDto>> CreateOrder(
            CreateOrderDto dto)
        {
            var buyer = await _context.Users
                .FirstOrDefaultAsync(u =>
                    u.Id == dto.BuyerId &&
                    u.Role == UserRole.Buyer);

            if (buyer == null)
            {
                return ServiceResult<OrderResponseDto>.Failure(
                    ServiceError.InvalidReference,
                    "The specified buyer does not exist.");
            }

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == dto.ProductId);

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
                CreatedAt = DateTime.UtcNow
            };

            _context.Orders.Add(order);

            await _context.SaveChangesAsync();

            // Product is already available, so we can map the result directly.
            order.Product = product;

            return ServiceResult<OrderResponseDto>.Success(
                MapToDto(order));
        }

        public async Task<ServiceResult<bool>> UpdateOrderStatus(
            int id,
            UpdateOrderStatusDto dto)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == id);

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

            await _context.SaveChangesAsync();

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
