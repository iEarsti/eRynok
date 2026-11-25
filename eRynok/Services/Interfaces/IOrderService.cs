using eRynok.DTOs.Orders;
using eRynok.DTOs.OrderGroups;
using eRynok.Models;

namespace eRynok.Services.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderResponseDto>> GetOrders(
            int? buyerId,
            int? sellerId,
            OrderStatus? status,
            int currentUserId,
            UserRole currentUserRole);

        Task<ServiceResult<OrderResponseDto>> GetOrder(
            int id,
            int currentUserId,
            UserRole currentUserRole);

        Task<ServiceResult<OrderGroupResponseDto>> GetCart(
            int currentUserId);

        Task<ServiceResult<OrderGroupResponseDto>> CreateOrder(
            CreateOrderDto dto,
            int currentUserId);

        Task<ServiceResult<bool>> UpdateOrderStatus(
            int id,
            UpdateOrderStatusDto dto,
            int currentUserId,
            UserRole currentUserRole);

        Task<ServiceResult<bool>> DeleteOrder(
            int id,
            int currentUserId,
            UserRole currentUserRole);
    }
}