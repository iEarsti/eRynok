using eRynok.DTOs.Categories;
namespace eRynok.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<IEnumerable<CategoryResponseDto>> GetCategories(string? search);
        Task<CategoryResponseDto?> GetCategory(int id);
        Task<ServiceResult<CategoryResponseDto>> CreateCategory(
            CreateCategoryDto dto);
        Task<ServiceResult<bool>> UpdateCategory(
            int id,
            UpdateCategoryDto dto);
        Task<ServiceResult<bool>> DeleteCategory(int id);
    }
}
