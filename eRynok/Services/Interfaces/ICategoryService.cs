using eRynok.DTOs.Categories;
namespace eRynok.Services.Interfaces
{
    public interface ICategoryService
    {
        IEnumerable<CategoryResponseDto> GetCategories(string? search);

        CategoryResponseDto? GetCategory(int id);

        ServiceResult<CategoryResponseDto> CreateCategory(
            CreateCategoryDto dto);

        ServiceResult<bool> UpdateCategory(
            int id,
            UpdateCategoryDto dto);

        ServiceResult<bool> DeleteCategory(int id);
    }
}
