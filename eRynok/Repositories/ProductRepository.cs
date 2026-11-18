using eRynok.Data;
using eRynok.Models;
using eRynok.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace eRynok.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly eRynokDbContext _context;

        public ProductRepository(eRynokDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Product>> GetProducts(
            string? search,
            int? categoryId,
            int? userId,
            decimal? minPrice,
            decimal? maxPrice)
        {
            var query = _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Seller)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p =>
                    p.Name.Contains(search) ||
                    p.Description.Contains(search));
            }

            if (categoryId.HasValue)
                query = query.Where(p => p.CategoryId == categoryId.Value);

            if (userId.HasValue)
                query = query.Where(p => p.UserId == userId.Value);

            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            return await query.ToListAsync();
        }

        public async Task<Product?> GetProduct(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Seller)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<bool> CategoryExists(int categoryId)
        {
            return await _context.Categories
                .AnyAsync(c => c.Id == categoryId);
        }

        public async Task<bool> SellerExists(int userId)
        {
            return await (
                from userRole in _context.UserRoles
                join role in _context.Roles
                    on userRole.RoleId equals role.Id
                where userRole.UserId == userId
                      && role.Name == "Seller"
                select userRole
            ).AnyAsync();
        }

        public async Task<bool> HasOrders(int productId)
        {
            return await _context.Orders
                .AnyAsync(o => o.ProductId == productId);
        }

        public async Task AddProduct(Product product)
        {
            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateProduct(Product product)
        {
            _context.Products.Update(product);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteProduct(Product product)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
    }
}
