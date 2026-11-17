using eRynok.Data;
using eRynok.Models;
using eRynok.DTOs.Products;
using eRynok.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace eRynok.Services
{
    public class ProductService : IProductService
    {
        private readonly eRynokDbContext _context;

        public ProductService(eRynokDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProductResponseDto>> GetProducts(
            string? search,
            int? categoryId,
            int? userId,
            decimal? minPrice,
            decimal? maxPrice)
        {
            var query = _context.Products.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(p =>
                    p.Name.Contains(search) ||
                    p.Description.Contains(search));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p =>
                    p.CategoryId == categoryId.Value);
            }

            if (userId.HasValue)
            {
                query = query.Where(p =>
                    p.UserId == userId.Value);
            }

            if (minPrice.HasValue)
            {
                query = query.Where(p =>
                    p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                query = query.Where(p =>
                    p.Price <= maxPrice.Value);
            }

            var products = await query.ToListAsync();

            return products.Select(MapToDto).ToList();
        }

        public async Task<ProductResponseDto?> GetProduct(int id)
        {
            var product = await _context.Products
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id);

            return product == null ? null : MapToDto(product);
        }

        public async Task<ServiceResult<ProductResponseDto>> CreateProduct(
            CreateProductDto dto)
        {
            bool categoryExists = await _context.Categories
                .AnyAsync(c => c.Id == dto.CategoryId);

            if (!categoryExists)
            {
                return ServiceResult<ProductResponseDto>.Failure(
                    ServiceError.InvalidReference,
                    $"Category with ID {dto.CategoryId} does not exist.");
            }

            bool sellerExists = await _context.Users
                .AnyAsync(u =>
                    u.Id == dto.UserId &&
                    u.Role == UserRole.Seller);

            if (!sellerExists)
            {
                return ServiceResult<ProductResponseDto>.Failure(
                    ServiceError.InvalidReference,
                    $"Seller with ID {dto.UserId} does not exist.");
            }

            var product = new Product
            {
                Name = dto.Name.Trim(),
                Description = dto.Description.Trim(),
                Price = dto.Price,
                ImageUrl = dto.ImageUrl,
                CategoryId = dto.CategoryId,
                UserId = dto.UserId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);

            await _context.SaveChangesAsync();

            return ServiceResult<ProductResponseDto>.Success(
                MapToDto(product));
        }

        public async Task<ServiceResult<bool>> UpdateProduct(
            int id,
            UpdateProductDto dto)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return ServiceResult<bool>.Failure(
                    ServiceError.NotFound,
                    $"Product with ID {id} was not found.");
            }

            bool categoryExists = await _context.Categories
                .AnyAsync(c => c.Id == dto.CategoryId);

            if (!categoryExists)
            {
                return ServiceResult<bool>.Failure(
                    ServiceError.InvalidReference,
                    $"Category with ID {dto.CategoryId} does not exist.");
            }

            bool sellerExists = await _context.Users
                .AnyAsync(u =>
                    u.Id == dto.UserId &&
                    u.Role == UserRole.Seller);

            if (!sellerExists)
            {
                return ServiceResult<bool>.Failure(
                    ServiceError.InvalidReference,
                    $"Seller with ID {dto.UserId} does not exist.");
            }

            product.Name = dto.Name.Trim();
            product.Description = dto.Description.Trim();
            product.Price = dto.Price;
            product.ImageUrl = dto.ImageUrl;
            product.CategoryId = dto.CategoryId;
            product.UserId = dto.UserId;

            await _context.SaveChangesAsync();

            return ServiceResult<bool>.Success(true);
        }

        public async Task<ServiceResult<bool>> DeleteProduct(int id)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return ServiceResult<bool>.Failure(
                    ServiceError.NotFound,
                    $"Product with ID {id} was not found.");
            }

            bool hasOrders = await _context.Orders
                .AnyAsync(o => o.ProductId == id);

            if (hasOrders)
            {
                return ServiceResult<bool>.Failure(
                    ServiceError.Conflict,
                    "Cannot delete product because it has existing orders.");
            }

            _context.Products.Remove(product);

            await _context.SaveChangesAsync();

            return ServiceResult<bool>.Success(true);
        }

        private static ProductResponseDto MapToDto(Product product)
        {
            return new ProductResponseDto
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                ImageUrl = product.ImageUrl,
                CategoryId = product.CategoryId,
                UserId = product.UserId,
                CreatedAt = product.CreatedAt
            };
        }
    }
}
