using eRynok.Models;

namespace eRynok.Repositories.Interfaces
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Category>> GetCategories(string? search);

        Task<Category?> GetCategory(int id);

        Task<bool> CategoryExists(string name, int? excludedId = null);

        Task<bool> HasProducts(int categoryId);

        Task AddCategory(Category category);

        Task UpdateCategory(Category category);

        Task DeleteCategory(Category category);
    }
}
