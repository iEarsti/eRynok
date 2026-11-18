using eRynok.Models;

namespace eRynok.Repositories.Interfaces
{
    public interface IOrderRepository
    {
        Task<IEnumerable<Order>> GetOrders(
            int? buyerId,
            int? sellerId,
            OrderStatus? status);

        Task<Order?> GetOrder(int id);

        Task<bool> BuyerExists(int buyerId);

        Task<bool> ProductExists(int productId);

        Task AddOrder(Order order);

        Task UpdateOrder(Order order);

        Task DeleteOrder(Order order);
    }
}
