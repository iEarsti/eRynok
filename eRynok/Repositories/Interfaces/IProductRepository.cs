using eRynok.Models;

namespace eRynok.Repositories.Interfaces
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetProducts(
            string? search,
            int? categoryId,
            int? userId,
            decimal? minPrice,
            decimal? maxPrice);

        Task<Product?> GetProduct(int id);

        Task<bool> CategoryExists(int categoryId);

        Task<bool> SellerExists(int userId);

        Task<bool> HasOrders(int productId);

        Task AddProduct(Product product);

        Task UpdateProduct(Product product);

        Task DeleteProduct(Product product);
    }
}
