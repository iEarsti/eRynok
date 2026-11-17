using eRynok.Data;
using eRynok.Models;
using eRynok.DTOs.Categories;
using eRynok.Services.Interfaces;

namespace eRynok.Services
{
    public class CategoryService : ICategoryService
    {
        public IEnumerable<CategoryResponseDto> GetCategories(string? search)
        {
            var categories = eRynokData.Categories.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                categories = categories.Where(c =>
                    c.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    c.Description.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            return categories.Select(MapToDto).ToList();
        }

        public CategoryResponseDto? GetCategory(int id)
        {
            var category = eRynokData.Categories
                .FirstOrDefault(c => c.Id == id);

            return category == null ? null : MapToDto(category);
        }

        public ServiceResult<CategoryResponseDto> CreateCategory(
            CreateCategoryDto dto)
        {
            string name = dto.Name.Trim();

            bool categoryExists = eRynokData.Categories.Any(c =>
                c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (categoryExists)
            {
                return ServiceResult<CategoryResponseDto>.Failure(
                    ServiceError.Conflict,
                    "Category with this name already exists.");
            }

            var category = new Category
            {
                Id = eRynokData.Categories.Count > 0
                    ? eRynokData.Categories.Max(c => c.Id) + 1
                    : 1,

                Name = name,
                Description = dto.Description.Trim()
            };

            eRynokData.Categories.Add(category);
            eRynokData.SaveCategories();

            return ServiceResult<CategoryResponseDto>.Success(
                MapToDto(category));
        }

        public ServiceResult<bool> UpdateCategory(
            int id,
            UpdateCategoryDto dto)
        {
            var category = eRynokData.Categories
                .FirstOrDefault(c => c.Id == id);

            if (category == null)
            {
                return ServiceResult<bool>.Failure(
                    ServiceError.NotFound,
                    $"Category with ID {id} was not found.");
            }

            string name = dto.Name.Trim();

            bool categoryExists = eRynokData.Categories.Any(c =>
                c.Id != id &&
                c.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

            if (categoryExists)
            {
                return ServiceResult<bool>.Failure(
                    ServiceError.Conflict,
                    "Another category with this name already exists.");
            }

            category.Name = name;
            category.Description = dto.Description.Trim();

            eRynokData.SaveCategories();

            return ServiceResult<bool>.Success(true);
        }

        public ServiceResult<bool> DeleteCategory(int id)
        {
            var category = eRynokData.Categories
                .FirstOrDefault(c => c.Id == id);

            if (category == null)
            {
                return ServiceResult<bool>.Failure(
                    ServiceError.NotFound,
                    $"Category with ID {id} was not found.");
            }

            bool hasProducts = eRynokData.Products
                .Any(p => p.CategoryId == id);

            if (hasProducts)
            {
                return ServiceResult<bool>.Failure(
                    ServiceError.Conflict,
                    "Cannot delete category because it contains products.");
            }

            eRynokData.Categories.Remove(category);
            eRynokData.SaveCategories();

            return ServiceResult<bool>.Success(true);
        }

        private static CategoryResponseDto MapToDto(Category category)
        {
            return new CategoryResponseDto
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description
            };
        }
    }
}
