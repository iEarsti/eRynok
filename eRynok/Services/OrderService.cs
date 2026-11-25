using eRynok.DTOs.Orders;
using eRynok.DTOs.OrderGroups;
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
            OrderStatus? status,
            int currentUserId,
            UserRole currentUserRole)
        {
            if (currentUserRole == UserRole.Buyer)
            {
                buyerId = currentUserId;
                sellerId = null;
            }
            else if (currentUserRole == UserRole.Seller)
            {
                sellerId = currentUserId;
                buyerId = null;
            }

            var orders = await _orderRepository.GetOrders(
                buyerId,
                sellerId,
                status);

            return orders.Select(MapToDto).ToList();
        }

        public async Task<ServiceResult<OrderResponseDto>> GetOrder(
            int id,
            int currentUserId,
            UserRole currentUserRole)
        {
            var order = await _orderRepository.GetOrder(id);

            if (order == null)
            {
                return ServiceResult<OrderResponseDto>.Failure(
                    ServiceError.NotFound,
                    $"Order with ID {id} was not found.");
            }

            bool hasAccess =
                currentUserRole == UserRole.Admin ||
                (currentUserRole == UserRole.Buyer &&
                 order.BuyerId == currentUserId) ||
                (currentUserRole == UserRole.Seller &&
                 order.Product.UserId == currentUserId);

            if (!hasAccess)
            {
                return ServiceResult<OrderResponseDto>.Failure(
                    ServiceError.Forbidden,
                    "You do not have permission to view this order.");
            }

            return ServiceResult<OrderResponseDto>.Success(
                MapToDto(order));
        }

        public async Task<ServiceResult<OrderGroupResponseDto>> GetCart(
            int currentUserId)
        {
            var orderGroup = await _orderRepository.GetOrderGroupByBuyerId(
                currentUserId);

            if (orderGroup == null)
            {
                return ServiceResult<OrderGroupResponseDto>.Failure(
                    ServiceError.NotFound,
                    "Cart not found.");
            }

            var orders = orderGroup.Orders
                .Select(MapToDto)
                .ToList();

            var response = new OrderGroupResponseDto
            {
                Id = orderGroup.Id,
                BuyerId = orderGroup.BuyerId,
                CreatedAt = orderGroup.CreatedAt,
                TotalAmount = orders.Sum(order => order.TotalPrice),
                Orders = orders
            };

            return ServiceResult<OrderGroupResponseDto>.Success(response);
        }

        public async Task<ServiceResult<OrderGroupResponseDto>> CreateOrder(
            CreateOrderDto dto,
            int currentUserId)
        {
            bool buyerExists = await _orderRepository.BuyerExists(
                currentUserId);

            if (!buyerExists)
            {
                return ServiceResult<OrderGroupResponseDto>.Failure(
                    ServiceError.InvalidReference,
                    "The specified buyer does not exist.");
            }

            if (dto.Items == null || dto.Items.Count == 0)
            {
                return ServiceResult<OrderGroupResponseDto>.Failure(
                    ServiceError.InvalidReference,
                    "The order must contain at least one product.");
            }

            if (dto.Items.Any(item =>
                item.Quantity < 1 || item.Quantity > 1000))
            {
                return ServiceResult<OrderGroupResponseDto>.Failure(
                    ServiceError.InvalidReference,
                    "Each product quantity must be between 1 and 1000.");
            }

            if (dto.Items.GroupBy(item => item.ProductId)
                .Any(group => group.Count() > 1))
            {
                return ServiceResult<OrderGroupResponseDto>.Failure(
                    ServiceError.InvalidReference,
                    "The same product cannot appear more than once in an order.");
            }

            var orders = new List<Order>();
            DateTime createdAt = DateTime.UtcNow;

            foreach (var item in dto.Items)
            {
                var product = await _productRepository.GetProduct(
                    item.ProductId);

                if (product == null)
                {
                    return ServiceResult<OrderGroupResponseDto>.Failure(
                        ServiceError.InvalidReference,
                        $"Product with ID {item.ProductId} does not exist.");
                }

                var order = new Order
                {
                    BuyerId = currentUserId,
                    ProductId = product.Id,
                    Quantity = item.Quantity,
                    UnitPrice = product.Price,
                    Status = OrderStatus.Pending,
                    CreatedAt = createdAt,
                    Product = product
                };

                orders.Add(order);
            }

            var orderGroup = await _orderRepository.GetOrderGroupByBuyerId(
                currentUserId);

            if (orderGroup == null)
            {
                orderGroup = new OrderGroup
                {
                    BuyerId = currentUserId,
                    CreatedAt = createdAt,
                    Orders = orders
                };

                await _orderRepository.AddOrderGroup(orderGroup);
            }
            else
            {
                foreach (var order in orders)
                {
                    order.OrderGroupId = orderGroup.Id;
                }

                await _orderRepository.AddOrders(orders);
            }

            var responseOrders = orderGroup.Orders
                .Concat(orders)
                .DistinctBy(order => order.Id)
                .Select(MapToDto)
                .ToList();

            var response = new OrderGroupResponseDto
            {
                Id = orderGroup.Id,
                BuyerId = orderGroup.BuyerId,
                CreatedAt = orderGroup.CreatedAt,
                TotalAmount = responseOrders.Sum(order => order.TotalPrice),
                Orders = responseOrders
            };

            return ServiceResult<OrderGroupResponseDto>.Success(response);
        }

        public async Task<ServiceResult<bool>> UpdateOrderStatus(
            int id,
            UpdateOrderStatusDto dto,
            int currentUserId,
            UserRole currentUserRole)
        {
            var order = await _orderRepository.GetOrder(id);

            if (order == null)
            {
                return ServiceResult<bool>.Failure(
                    ServiceError.NotFound,
                    $"Order with ID {id} was not found.");
            }

            if (currentUserRole != UserRole.Admin &&
                currentUserRole != UserRole.Seller)
            {
                return ServiceResult<bool>.Failure(
                    ServiceError.Forbidden,
                    "You do not have permission to update order status.");
            }

            if (currentUserRole == UserRole.Seller &&
                order.Product.UserId != currentUserId)
            {
                return ServiceResult<bool>.Failure(
                    ServiceError.Forbidden,
                    "You do not have permission to update this order.");
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

        public async Task<ServiceResult<bool>> DeleteOrder(
            int id,
            int currentUserId,
            UserRole currentUserRole)
        {
            var order = await _orderRepository.GetOrder(id);

            if (order == null)
            {
                return ServiceResult<bool>.Failure(
                    ServiceError.NotFound,
                    "Order not found.");
            }

            if (currentUserRole != UserRole.Admin &&
                (currentUserRole != UserRole.Buyer ||
                 order.BuyerId != currentUserId))
            {
                return ServiceResult<bool>.Failure(
                    ServiceError.Forbidden,
                    "You can only delete your own orders.");
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
