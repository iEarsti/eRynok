using eRynok.Data;
using eRynok.Models;
using eRynok.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;


namespace eRynok.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly eRynokDbContext _context;

        public UserRepository(eRynokDbContext context)
        {
            _context = context;
        }

        public async Task<ApplicationUser?> GetUser(int id)
        {
            return await _context.Users
                .Include(u => u.SellerProfile)
                .Include(u => u.BuyerProfile)
                .FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<IEnumerable<ApplicationUser>> GetUsers(
            string? search,
            string? role)
        {
            var query = _context.Users
                .AsNoTracking()
                .Include(u => u.SellerProfile)
                .Include(u => u.BuyerProfile)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(u =>
                    u.Name.Contains(search) ||
                    (u.Email != null && u.Email.Contains(search)));
            }

            if (!string.IsNullOrWhiteSpace(role))
            {
                var usersWithRole =
                    from userRole in _context.UserRoles
                    join identityRole in _context.Roles
                        on userRole.RoleId equals identityRole.Id
                    where identityRole.Name == role
                    select userRole.UserId;

                query = query.Where(u => usersWithRole.Contains(u.Id));
            }

            return await query.ToListAsync();
        }

        public async Task<bool> HasProducts(int userId)
        {
            return await _context.Products
                .AnyAsync(p => p.UserId == userId);
        }

        public async Task<bool> HasOrders(int userId)
        {
            return await _context.Orders
                .AnyAsync(o => o.BuyerId == userId);
        }
    }
}
