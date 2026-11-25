using eRynok.Data;
using eRynok.Models;
using eRynok.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace eRynok.Repositories
{
    public class OrderRepository : IOrderRepository
    {
        private readonly eRynokDbContext _context;

        public OrderRepository(eRynokDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Order>> GetOrders(
            int? buyerId,
            int? sellerId,
            OrderStatus? status)
        {
            var query = _context.Orders
                .AsNoTracking()
                .Include(o => o.Buyer)
                .Include(o => o.Product)
                    .ThenInclude(p => p.Seller)
                .Include(o => o.Product)
                    .ThenInclude(p => p.Category)
                .AsQueryable();

            if (buyerId.HasValue)
                query = query.Where(o => o.BuyerId == buyerId.Value);

            if (sellerId.HasValue)
                query = query.Where(o => o.Product.UserId == sellerId.Value);

            if (status.HasValue)
                query = query.Where(o => o.Status == status.Value);

            return await query.ToListAsync();
        }

        public async Task<Order?> GetOrder(int id)
        {
            return await _context.Orders
                .Include(o => o.Buyer)
                .Include(o => o.Product)
                    .ThenInclude(p => p.Seller)
                .Include(o => o.Product)
                    .ThenInclude(p => p.Category)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<OrderGroup?> GetOrderGroupByBuyerId(int buyerId)
        {
            return await _context.OrderGroups
                .Include(og => og.Orders)
                    .ThenInclude(o => o.Product)
                .FirstOrDefaultAsync(og => og.BuyerId == buyerId);
        }

        public async Task<bool> BuyerExists(int buyerId)
        {
            return await (
                from userRole in _context.UserRoles
                join role in _context.Roles
                    on userRole.RoleId equals role.Id
                where userRole.UserId == buyerId
                      && role.Name == "Buyer"
                select userRole
            ).AnyAsync();
        }

        public async Task<bool> ProductExists(int productId)
        {
            return await _context.Products
                .AnyAsync(p => p.Id == productId);
        }

        public async Task AddOrder(Order order)
        {
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
        }

        public async Task AddOrders(IEnumerable<Order> orders)
        {
            await _context.Orders.AddRangeAsync(orders);
            await _context.SaveChangesAsync();
        }

        public async Task AddOrderGroup(OrderGroup orderGroup)
        {
            await _context.OrderGroups.AddAsync(orderGroup);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateOrder(Order order)
        {
            using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                int? orderGroupId = order.OrderGroupId;

                if (orderGroupId.HasValue &&
                    order.Status != OrderStatus.Pending)
                {
                    order.OrderGroupId = null;
                }

                await _context.SaveChangesAsync();

                if (orderGroupId.HasValue &&
                    order.Status != OrderStatus.Pending)
                {
                    bool hasPendingOrders = await _context.Orders
                        .AnyAsync(o =>
                            o.OrderGroupId == orderGroupId.Value &&
                            o.Status == OrderStatus.Pending);

                    if (!hasPendingOrders)
                    {
                        var orderGroup = await _context.OrderGroups
                            .FindAsync(orderGroupId.Value);

                        if (orderGroup != null)
                        {
                            _context.OrderGroups.Remove(orderGroup);
                            await _context.SaveChangesAsync();
                        }
                    }
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        public async Task DeleteOrder(Order order)
        {
            using var transaction =
                await _context.Database.BeginTransactionAsync();

            try
            {
                int? orderGroupId = order.OrderGroupId;

                _context.Orders.Remove(order);
                await _context.SaveChangesAsync();

                if (orderGroupId.HasValue)
                {
                    bool hasOrders = await _context.Orders
                        .AnyAsync(o => o.OrderGroupId == orderGroupId.Value);

                    if (!hasOrders)
                    {
                        var orderGroup = await _context.OrderGroups
                            .FindAsync(orderGroupId.Value);

                        if (orderGroup != null)
                        {
                            _context.OrderGroups.Remove(orderGroup);
                            await _context.SaveChangesAsync();
                        }
                    }
                }

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
    }
}
