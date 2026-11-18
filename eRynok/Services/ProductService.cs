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
            CreateProductDto dto)
        {
            bool categoryExists = await _productRepository.CategoryExists(
                dto.CategoryId);

            if (!categoryExists)
            {
                return ServiceResult<ProductResponseDto>.Failure(
                    ServiceError.InvalidReference,
                    $"Category with ID {dto.CategoryId} does not exist.");
            }

            bool sellerExists = await _productRepository.SellerExists(
                dto.UserId);

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

            await _productRepository.AddProduct(product);

            return ServiceResult<ProductResponseDto>.Success(
                MapToDto(product));
        }

        public async Task<ServiceResult<bool>> UpdateProduct(
            int id,
            UpdateProductDto dto)
        {
            var product = await _productRepository.GetProduct(id);

            if (product == null)
            {
                return ServiceResult<bool>.Failure(
                    ServiceError.NotFound,
                    $"Product with ID {id} was not found.");
            }

            bool categoryExists = await _productRepository.CategoryExists(
                dto.CategoryId);

            if (!categoryExists)
            {
                return ServiceResult<bool>.Failure(
                    ServiceError.InvalidReference,
                    $"Category with ID {dto.CategoryId} does not exist.");
            }

            bool sellerExists = await _productRepository.SellerExists(
                dto.UserId);

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

            await _productRepository.UpdateProduct(product);

            return ServiceResult<bool>.Success(true);
        }

        public async Task<ServiceResult<bool>> DeleteProduct(int id)
        {
            var product = await _productRepository.GetProduct(id);

            if (product == null)
            {
                return ServiceResult<bool>.Failure(
                    ServiceError.NotFound,
                    $"Product with ID {id} was not found.");
            }

            bool hasOrders = await _productRepository.HasOrders(id);

            if (hasOrders)
            {
                return ServiceResult<bool>.Failure(
                    ServiceError.Conflict,
                    "Cannot delete product because it has existing orders.");
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
