using eRynok.Models;
using eRynok.DTOs.Categories;
using eRynok.Services.Interfaces;
using eRynok.Repositories.Interfaces;

namespace eRynok.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;

        public CategoryService(ICategoryRepository categoryRepository)
        {
            _categoryRepository = categoryRepository;
        }

        public async Task<IEnumerable<CategoryResponseDto>> GetCategories(
            string? search)
        {
            search = search?.Trim();

            var categories = await _categoryRepository.GetCategories(search);

            return categories.Select(MapToDto).ToList();
        }

        public async Task<CategoryResponseDto?> GetCategory(int id)
        {
            var category = await _categoryRepository.GetCategory(id);

            return category == null ? null : MapToDto(category);
        }

        public async Task<ServiceResult<CategoryResponseDto>> CreateCategory(
            CreateCategoryDto dto)
        {
            string name = dto.Name.Trim();

            bool categoryExists = await _categoryRepository.CategoryExists(name);

            if (categoryExists)
            {
                return ServiceResult<CategoryResponseDto>.Failure(
                    ServiceError.Conflict,
                    "Category with this name already exists.");
            }

            var category = new Category
            {
                Name = name,
                Description = dto.Description.Trim()
            };

            await _categoryRepository.AddCategory(category);

            return ServiceResult<CategoryResponseDto>.Success(
                MapToDto(category));
        }

        public async Task<ServiceResult<bool>> UpdateCategory(
            int id,
            UpdateCategoryDto dto)
        {
            var category = await _categoryRepository.GetCategory(id);

            if (category == null)
            {
                return ServiceResult<bool>.Failure(
                    ServiceError.NotFound,
                    $"Category with ID {id} was not found.");
            }

            string name = dto.Name.Trim();

            bool categoryExists = await _categoryRepository.CategoryExists(
                name,
                id);

            if (categoryExists)
            {
                return ServiceResult<bool>.Failure(
                    ServiceError.Conflict,
                    "Another category with this name already exists.");
            }

            category.Name = name;
            category.Description = dto.Description.Trim();

            await _categoryRepository.UpdateCategory(category);

            return ServiceResult<bool>.Success(true);
        }

        public async Task<ServiceResult<bool>> DeleteCategory(int id)
        {
            var category = await _categoryRepository.GetCategory(id);

            if (category == null)
            {
                return ServiceResult<bool>.Failure(
                    ServiceError.NotFound,
                    $"Category with ID {id} was not found.");
            }

            bool hasProducts = await _categoryRepository.HasProducts(id);

            if (hasProducts)
            {
                return ServiceResult<bool>.Failure(
                    ServiceError.Conflict,
                    "Cannot delete category because it contains products.");
            }

            await _categoryRepository.DeleteCategory(category);

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
