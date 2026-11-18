using eRynok.Models;

namespace eRynok.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<ApplicationUser?> GetUser(int id);

        Task<IEnumerable<ApplicationUser>> GetUsers(
            string? search,
            string? role);

        Task<bool> HasProducts(int userId);

        Task<bool> HasOrders(int userId);
    }
}
