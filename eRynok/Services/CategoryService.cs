using eRynok.Data;
using eRynok.Models;
using eRynok.DTOs.Categories;
using eRynok.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace eRynok.Services
{
    public class CategoryService : ICategoryService
{
    private readonly eRynokDbContext _context;

    public CategoryService(eRynokDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<CategoryResponseDto>> GetCategories(string? search)
    {
        var query = _context.Categories.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();

            query = query.Where(c =>
                c.Name.Contains(search) ||
                c.Description.Contains(search));
        }

        var categories = await query.ToListAsync();

        return categories.Select(MapToDto).ToList();
    }

    public async Task<CategoryResponseDto?> GetCategory(int id)
    {
        var category = await _context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);

        return category == null ? null : MapToDto(category);
    }

    public async Task<ServiceResult<CategoryResponseDto>> CreateCategory(
        CreateCategoryDto dto)
    {
        string name = dto.Name.Trim();

        bool categoryExists = await _context.Categories
            .AnyAsync(c => c.Name == name);

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

        _context.Categories.Add(category);

        await _context.SaveChangesAsync();

        return ServiceResult<CategoryResponseDto>.Success(
            MapToDto(category));
    }

    public async Task<ServiceResult<bool>> UpdateCategory(
        int id,
        UpdateCategoryDto dto)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
        {
            return ServiceResult<bool>.Failure(
                ServiceError.NotFound,
                $"Category with ID {id} was not found.");
        }

        string name = dto.Name.Trim();

        bool categoryExists = await _context.Categories
            .AnyAsync(c => c.Id != id && c.Name == name);

        if (categoryExists)
        {
            return ServiceResult<bool>.Failure(
                ServiceError.Conflict,
                "Another category with this name already exists.");
        }

        category.Name = name;
        category.Description = dto.Description.Trim();

        await _context.SaveChangesAsync();

        return ServiceResult<bool>.Success(true);
    }

    public async Task<ServiceResult<bool>> DeleteCategory(int id)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
        {
            return ServiceResult<bool>.Failure(
                ServiceError.NotFound,
                $"Category with ID {id} was not found.");
        }

        bool hasProducts = await _context.Products
            .AnyAsync(p => p.CategoryId == id);

        if (hasProducts)
        {
            return ServiceResult<bool>.Failure(
                ServiceError.Conflict,
                "Cannot delete category because it contains products.");
        }

        _context.Categories.Remove(category);

        await _context.SaveChangesAsync();

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
