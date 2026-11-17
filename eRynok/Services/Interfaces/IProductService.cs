using eRynok.DTOs.Products;

namespace eRynok.Services.Interfaces
{
    public interface IProductService
    {
        IEnumerable<ProductResponseDto> GetProducts(
            string? search,
            int? categoryId,
            int? userId,
            decimal? minPrice,
            decimal? maxPrice);

        ProductResponseDto? GetProduct(int id);

        ServiceResult<ProductResponseDto> CreateProduct(
            CreateProductDto dto);

        ServiceResult<bool> UpdateProduct(
            int id,
            UpdateProductDto dto);

        ServiceResult<bool> DeleteProduct(int id);
    }
}
