using eRynok.Models;
using eRynok.DTOs.Products;
using eRynok.Services.Interfaces;
using eRynok.Repositories.Interfaces;

namespace eRynok.Services
{
    public class ProductService : IProductService
    {
        private readonly IProductRepository _productRepository;

        public ProductService(IProductRepository productRepository)
        {
            _productRepository = productRepository;
        }

        public async Task<IEnumerable<ProductResponseDto>> GetProducts(
            string? search,
            int? categoryId,
            int? userId,
            decimal? minPrice,
            decimal? maxPrice)
        {
            search = search?.Trim();

            var products = await _productRepository.GetProducts(
                search,
                categoryId,
                userId,
                minPrice,
                maxPrice);

            return products.Select(MapToDto).ToList();
        }

        public async Task<ProductResponseDto?> GetProduct(int id)
        {
            var product = await _productRepository.GetProduct(id);

            return product == null ? null : MapToDto(product);
        }

        public async Task<ServiceResult<ProductResponseDto>> CreateProduct(
            CreateProductDto dto,
            int currentUserId,
            bool isAdmin)
        {
            bool categoryExists = await _productRepository.CategoryExists(dto.CategoryId);

            if (!categoryExists)
            {
                return ServiceResult<ProductResponseDto>.Failure(
                    ServiceError.InvalidReference,
                    "Категорію не знайдено.");
            }

            int sellerId;

            if (isAdmin)
            {
                if (!dto.UserId.HasValue)
                {
                    return ServiceResult<ProductResponseDto>.Failure(
                        ServiceError.InvalidReference,
                        "Адміністратор повинен вказати продавця.");
                }

                sellerId = dto.UserId.Value;
            }
            else
            {
                sellerId = currentUserId;
            }

            bool sellerExists = await _productRepository.SellerExists(sellerId);

            if (!sellerExists)
            {
                return ServiceResult<ProductResponseDto>.Failure(
                    ServiceError.InvalidReference,
                    "Продавця не знайдено.");
            }

            var product = new Product
            {
                Name = dto.Name.Trim(),
                Description = dto.Description.Trim(),
                Price = dto.Price,
                ImageUrl = dto.ImageUrl,
                CategoryId = dto.CategoryId,
                UserId = sellerId,
                CreatedAt = DateTime.UtcNow
            };

            await _productRepository.AddProduct(product);

            return ServiceResult<ProductResponseDto>.Success(MapToDto(product));
        }

        public async Task<ServiceResult<bool>> UpdateProduct(
    int id,
    UpdateProductDto dto,
    int currentUserId,
    bool isAdmin)
        {
            var product = await _productRepository.GetProduct(id);

            if (product == null)
            {
                return ServiceResult<bool>.Failure(
                    ServiceError.NotFound,
                    "Товар не знайдено.");
            }

            if (!isAdmin && product.UserId != currentUserId)
            {
                return ServiceResult<bool>.Failure(
                    ServiceError.Forbidden,
                    "Ви не маєте права редагувати цей товар.");
            }

            bool categoryExists = await _productRepository.CategoryExists(dto.CategoryId);

            if (!categoryExists)
            {
                return ServiceResult<bool>.Failure(
                    ServiceError.InvalidReference,
                    "Категорію не знайдено.");
            }

            if (isAdmin && dto.UserId.HasValue)
            {
                bool sellerExists = await _productRepository.SellerExists(dto.UserId.Value);

                if (!sellerExists)
                {
                    return ServiceResult<bool>.Failure(
                        ServiceError.InvalidReference,
                        "Продавця не знайдено.");
                }

                product.UserId = dto.UserId.Value;
            }

            product.Name = dto.Name.Trim();
            product.Description = dto.Description.Trim();
            product.Price = dto.Price;
            product.ImageUrl = dto.ImageUrl;
            product.CategoryId = dto.CategoryId;

            await _productRepository.UpdateProduct(product);

            return ServiceResult<bool>.Success(true);
        }

        public async Task<ServiceResult<bool>> DeleteProduct(
            int id,
            int currentUserId,
            bool isAdmin)
        {
            var product = await _productRepository.GetProduct(id);

            if (product == null)
            {
                return ServiceResult<bool>.Failure(
                    ServiceError.NotFound,
                    "Товар не знайдено.");
            }

            if (!isAdmin && product.UserId != currentUserId)
            {
                return ServiceResult<bool>.Failure(
                    ServiceError.Forbidden,
                    "Ви не маєте права видаляти цей товар.");
            }

            bool hasOrders = await _productRepository.HasOrders(id);

            if (hasOrders)
            {
                return ServiceResult<bool>.Failure(
                    ServiceError.Conflict,
                    "Неможливо видалити товар, який присутній у замовленнях.");
            }

            await _productRepository.DeleteProduct(product);

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
