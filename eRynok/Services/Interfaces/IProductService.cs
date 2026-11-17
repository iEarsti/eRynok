using eRynok.DTOs.Products;

namespace eRynok.Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductResponseDto>> GetProducts(
            string? search,
            int? categoryId,
            int? userId,
            decimal? minPrice,
            decimal? maxPrice);

        Task<ProductResponseDto?> GetProduct(int id);

        Task<ServiceResult<ProductResponseDto>> CreateProduct(
            CreateProductDto dto);

        Task<ServiceResult<bool>> UpdateProduct(
            int id,
            UpdateProductDto dto);

        Task<ServiceResult<bool>> DeleteProduct(int id);
    }
}
