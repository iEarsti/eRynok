using eRynok.Data;
using eRynok.Models;
using eRynok.DTOs.Users;
using eRynok.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace eRynok.Services
{
    public class UserService : IUserService
    {
        private readonly eRynokDbContext _context;

        public UserService(eRynokDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UserResponseDto>> GetUsers(
            string? search,
            UserRole? role)
        {
            var query = _context.Users
                .Include(u => u.SellerProfile)
                .Include(u => u.BuyerProfile)
                .AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();

                query = query.Where(u =>
                    u.Name.Contains(search) ||
                    u.Email.Contains(search));
            }

            if (role.HasValue)
            {
                query = query.Where(u => u.Role == role.Value);
            }

            var users = await query.ToListAsync();

            return users.Select(MapToDto).ToList();
        }

        public async Task<UserResponseDto?> GetUser(int id)
        {
            var user = await _context.Users
                .Include(u => u.SellerProfile)
                .Include(u => u.BuyerProfile)
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == id);

            return user == null ? null : MapToDto(user);
        }

        public async Task<ServiceResult<UserResponseDto>> CreateUser(
            CreateUserDto dto)
        {
            string email = dto.Email.Trim();

            bool emailExists = await _context.Users
                .AnyAsync(u => u.Email == email);

            if (emailExists)
            {
                return ServiceResult<UserResponseDto>.Failure(
                    ServiceError.Conflict,
                    "A user with this email already exists.");
            }

            if (!ValidateProfile(dto.Role, dto.Phone, dto.Address, out string message))
            {
                return ServiceResult<UserResponseDto>.Failure(
                    ServiceError.InvalidReference,
                    message);
            }

            var user = new User
            {
                Name = dto.Name.Trim(),
                Email = email,
                PasswordHash = string.Empty,
                Role = dto.Role
            };

            if (dto.Role == UserRole.Seller)
            {
                user.SellerProfile = new SellerProfile
                {
                    Phone = dto.Phone!.Trim(),
                    Address = dto.Address!.Trim()
                };
            }
            else if (dto.Role == UserRole.Buyer)
            {
                user.BuyerProfile = new BuyerProfile
                {
                    Phone = dto.Phone!.Trim(),
                    Address = dto.Address!.Trim()
                };
            }

            _context.Users.Add(user);

            await _context.SaveChangesAsync();

            return ServiceResult<UserResponseDto>.Success(
                MapToDto(user));
        }

        public async Task<ServiceResult<bool>> UpdateUser(
            int id,
            UpdateUserDto dto)
        {
            var user = await _context.Users
                .Include(u => u.SellerProfile)
                .Include(u => u.BuyerProfile)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return ServiceResult<bool>.Failure(
                    ServiceError.NotFound,
                    $"User with ID {id} was not found.");
            }

            string email = dto.Email.Trim();

            bool emailExists = await _context.Users
                .AnyAsync(u => u.Id != id && u.Email == email);

            if (emailExists)
            {
                return ServiceResult<bool>.Failure(
                    ServiceError.Conflict,
                    "Another user with this email already exists.");
            }

            if (!ValidateProfile(dto.Role, dto.Phone, dto.Address, out string message))
            {
                return ServiceResult<bool>.Failure(
                    ServiceError.InvalidReference,
                    message);
            }

            bool hasProducts = await _context.Products
                .AnyAsync(p => p.UserId == id);

            if (hasProducts && dto.Role != UserRole.Seller)
            {
                return ServiceResult<bool>.Failure(
                    ServiceError.Conflict,
                    "Cannot change the role of a seller who has products.");
            }

            user.Name = dto.Name.Trim();
            user.Email = email;
            user.Role = dto.Role;

            UpdateProfile(user, dto.Role, dto.Phone, dto.Address);

            await _context.SaveChangesAsync();

            return ServiceResult<bool>.Success(true);
        }

        public async Task<ServiceResult<bool>> DeleteUser(int id)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == id);

            if (user == null)
            {
                return ServiceResult<bool>.Failure(
                    ServiceError.NotFound,
                    $"User with ID {id} was not found.");
            }

            bool hasProducts = await _context.Products
                .AnyAsync(p => p.UserId == id);

            if (hasProducts)
            {
                return ServiceResult<bool>.Failure(
                    ServiceError.Conflict,
                    "Cannot delete a user who owns products.");
            }

            bool hasOrders = await _context.Orders
                .AnyAsync(o => o.BuyerId == id);

            if (hasOrders)
            {
                return ServiceResult<bool>.Failure(
                    ServiceError.Conflict,
                    "Cannot delete a user who has placed orders.");
            }

            _context.Users.Remove(user);

            await _context.SaveChangesAsync();

            return ServiceResult<bool>.Success(true);
        }

        private static bool ValidateProfile(
            UserRole role,
            string? phone,
            string? address,
            out string message)
        {
            message = string.Empty;

            if (role == UserRole.Admin)
                return true;

            if (string.IsNullOrWhiteSpace(phone))
            {
                message = "Phone is required for sellers and buyers.";
                return false;
            }

            if (string.IsNullOrWhiteSpace(address))
            {
                message = "Address is required for sellers and buyers.";
                return false;
            }

            return true;
        }

        private static void UpdateProfile(
            User user,
            UserRole role,
            string? phone,
            string? address)
        {
            if (role == UserRole.Seller)
            {
                user.BuyerProfile = null;

                if (user.SellerProfile == null)
                {
                    user.SellerProfile = new SellerProfile();
                }

                user.SellerProfile.Phone = phone!.Trim();
                user.SellerProfile.Address = address!.Trim();
            }
            else if (role == UserRole.Buyer)
            {
                user.SellerProfile = null;

                if (user.BuyerProfile == null)
                {
                    user.BuyerProfile = new BuyerProfile();
                }

                user.BuyerProfile.Phone = phone!.Trim();
                user.BuyerProfile.Address = address!.Trim();
            }
            else
            {
                user.SellerProfile = null;
                user.BuyerProfile = null;
            }
        }

        private static UserResponseDto MapToDto(User user)
        {
            string? phone = null;
            string? address = null;

            if (user.Role == UserRole.Seller)
            {
                phone = user.SellerProfile?.Phone;
                address = user.SellerProfile?.Address;
            }
            else if (user.Role == UserRole.Buyer)
            {
                phone = user.BuyerProfile?.Phone;
                address = user.BuyerProfile?.Address;
            }

            return new UserResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Role = user.Role,
                Phone = phone,
                Address = address
            };
        }
    }
}
