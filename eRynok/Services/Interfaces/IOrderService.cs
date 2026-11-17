using eRynok.DTOs.Orders;
using eRynok.Models;

namespace eRynok.Services.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderResponseDto>> GetOrders(
            int? buyerId,
            int? sellerId,
            OrderStatus? status);

        Task<OrderResponseDto?> GetOrder(int id);

        Task<ServiceResult<OrderResponseDto>> CreateOrder(
            CreateOrderDto dto);

        Task<ServiceResult<bool>> UpdateOrderStatus(
            int id,
            UpdateOrderStatusDto dto);
    }
}
