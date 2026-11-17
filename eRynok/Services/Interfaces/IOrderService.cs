using eRynok.DTOs.Orders;
using eRynok.Models;

namespace eRynok.Services.Interfaces
{
    public interface IOrderService
    {
        IEnumerable<OrderResponseDto> GetOrders(
            int? buyerId,
            int? sellerId,
            OrderStatus? status);

        OrderResponseDto? GetOrder(int id);

        ServiceResult<OrderResponseDto> CreateOrder(CreateOrderDto dto);

        ServiceResult<bool> UpdateOrderStatus(int id, UpdateOrderStatusDto dto);
    }
}
