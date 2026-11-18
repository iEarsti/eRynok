using eRynok.Data;
using eRynok.Models;
using eRynok.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace eRynok.Repositories
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly eRynokDbContext _context;

        public CategoryRepository(eRynokDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Category>> GetCategories(string? search)
        {
            var query = _context.Categories
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(c =>
                    c.Name.Contains(search) ||
                    c.Description.Contains(search));
            }

            return await query.ToListAsync();
        }

        public async Task<Category?> GetCategory(int id)
        {
            return await _context.Categories
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<bool> CategoryExists(
            string name,
            int? excludedId = null)
        {
            var query = _context.Categories
                .Where(c => c.Name == name);

            if (excludedId.HasValue)
                query = query.Where(c => c.Id != excludedId.Value);

            return await query.AnyAsync();
        }

        public async Task<bool> HasProducts(int categoryId)
        {
            return await _context.Products
                .AnyAsync(p => p.CategoryId == categoryId);
        }

        public async Task AddCategory(Category category)
        {
            await _context.Categories.AddAsync(category);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCategory(Category category)
        {
            _context.Categories.Update(category);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCategory(Category category)
        {
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
        }
    }
}
