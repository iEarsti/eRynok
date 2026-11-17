using eRynok.Data;
using eRynok.Models;
using eRynok.DTOs.Products;
using eRynok.Services.Interfaces;

namespace eRynok.Services
{
    public class ProductService : IProductService
    {
        public IEnumerable<ProductResponseDto> GetProducts(
            string? search,
            int? categoryId,
            int? userId,
            decimal? minPrice,
            decimal? maxPrice)
        {
            var products = eRynokData.Products.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                products = products.Where(p =>
                    p.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    p.Description.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            if (categoryId.HasValue)
            {
                products = products.Where(p =>
                    p.CategoryId == categoryId.Value);
            }

            if (userId.HasValue)
            {
                products = products.Where(p =>
                    p.UserId == userId.Value);
            }

            if (minPrice.HasValue)
            {
                products = products.Where(p =>
                    p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue)
            {
                products = products.Where(p =>
                    p.Price <= maxPrice.Value);
            }

            return products.Select(MapToDto).ToList();
        }

        public ProductResponseDto? GetProduct(int id)
        {
            var product = eRynokData.Products
                .FirstOrDefault(p => p.Id == id);

            return product == null ? null : MapToDto(product);
        }

        public ServiceResult<ProductResponseDto> CreateProduct(
            CreateProductDto dto)
        {
            if (!eRynokData.Categories.Any(c =>
                c.Id == dto.CategoryId))
            {
                return ServiceResult<ProductResponseDto>.Failure(
                    ServiceError.InvalidReference,
                    $"Category with ID {dto.CategoryId} does not exist.");
            }

            if (!eRynokData.Users.Any(s =>
                s.Id == dto.UserId))
            {
                return ServiceResult<ProductResponseDto>.Failure(
                    ServiceError.InvalidReference,
                    $"Seller with ID {dto.UserId} does not exist.");
            }

            var product = new Product
            {
                Id = eRynokData.Products.Count > 0
                    ? eRynokData.Products.Max(p => p.Id) + 1
                    : 1,

                Name = dto.Name.Trim(),
                Description = dto.Description.Trim(),
                Price = dto.Price,
                ImageUrl = dto.ImageUrl,
                CategoryId = dto.CategoryId,
                UserId = dto.UserId,
                CreatedAt = DateTime.UtcNow
            };

            eRynokData.Products.Add(product);
            eRynokData.SaveProducts();

            return ServiceResult<ProductResponseDto>.Success(
                MapToDto(product));
        }

        public ServiceResult<bool> UpdateProduct(
            int id,
            UpdateProductDto dto)
        {
            var product = eRynokData.Products
                .FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return ServiceResult<bool>.Failure(
                    ServiceError.NotFound,
                    $"Product with ID {id} was not found.");
            }

            if (!eRynokData.Categories.Any(c =>
                c.Id == dto.CategoryId))
            {
                return ServiceResult<bool>.Failure(
                    ServiceError.InvalidReference,
                    $"Category with ID {dto.CategoryId} does not exist.");
            }

            if (!eRynokData.Users.Any(s =>
                s.Id == dto.UserId))
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

            eRynokData.SaveProducts();

            return ServiceResult<bool>.Success(true);
        }

        public ServiceResult<bool> DeleteProduct(int id)
        {
            var product = eRynokData.Products
                .FirstOrDefault(p => p.Id == id);

            if (product == null)
            {
                return ServiceResult<bool>.Failure(
                    ServiceError.NotFound,
                    $"Product with ID {id} was not found.");
            }

            eRynokData.Products.Remove(product);
            eRynokData.SaveProducts();

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
