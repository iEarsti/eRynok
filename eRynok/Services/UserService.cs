using eRynok.Data;
using eRynok.Models;
using eRynok.DTOs.Users;
using eRynok.Services.Interfaces;

namespace eRynok.Services
{
    public class UserService : IUserService
    {
        public IEnumerable<UserResponseDto> GetUsers(
            string? search,
            UserRole? role)
        {
            var users = eRynokData.Users.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                users = users.Where(u =>
                    u.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    u.Email.Contains(search, StringComparison.OrdinalIgnoreCase));
            }

            if (role.HasValue)
            {
                users = users.Where(u => u.Role == role.Value);
            }

            return users.Select(MapToDto).ToList();
        }

        public UserResponseDto? GetUser(int id)
        {
            var user = eRynokData.Users.FirstOrDefault(u => u.Id == id);

            return user == null ? null : MapToDto(user);
        }

        public ServiceResult<UserResponseDto> CreateUser(CreateUserDto dto)
        {
            string email = dto.Email.Trim();

            bool emailExists = eRynokData.Users.Any(u =>
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

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
                Id = eRynokData.Users.Count > 0
                    ? eRynokData.Users.Max(u => u.Id) + 1
                    : 1,

                Name = dto.Name.Trim(),
                Email = email,
                PasswordHash = string.Empty,
                Role = dto.Role
            };

            eRynokData.Users.Add(user);

            SaveProfile(user.Id, dto.Role, dto.Phone, dto.Address);

            eRynokData.SaveUsers();

            return ServiceResult<UserResponseDto>.Success(MapToDto(user));
        }

        public ServiceResult<bool> UpdateUser(int id, UpdateUserDto dto)
        {
            var user = eRynokData.Users.FirstOrDefault(u => u.Id == id);

            if (user == null)
            {
                return ServiceResult<bool>.Failure(
                    ServiceError.NotFound,
                    $"User with ID {id} was not found.");
            }

            string email = dto.Email.Trim();

            bool emailExists = eRynokData.Users.Any(u =>
                u.Id != id &&
                u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));

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

            bool hasProducts = eRynokData.Products.Any(p => p.UserId == id);

            if (hasProducts && dto.Role != UserRole.Seller)
            {
                return ServiceResult<bool>.Failure(
                    ServiceError.Conflict,
                    "Cannot change the role of a seller who has products.");
            }

            user.Name = dto.Name.Trim();
            user.Email = email;

            UpdateProfile(user.Id, dto.Role, dto.Phone, dto.Address);

            user.Role = dto.Role;

            eRynokData.SaveUsers();

            return ServiceResult<bool>.Success(true);
        }

        public ServiceResult<bool> DeleteUser(int id)
        {
            var user = eRynokData.Users.FirstOrDefault(u => u.Id == id);

            if (user == null)
            {
                return ServiceResult<bool>.Failure(
                    ServiceError.NotFound,
                    $"User with ID {id} was not found.");
            }

            bool hasProducts = eRynokData.Products.Any(p => p.UserId == id);

            if (hasProducts)
            {
                return ServiceResult<bool>.Failure(
                    ServiceError.Conflict,
                    "Cannot delete a user who owns products.");
            }

            eRynokData.Users.Remove(user);

            eRynokData.SellerProfiles.RemoveAll(p => p.UserId == id);
            eRynokData.BuyerProfiles.RemoveAll(p => p.UserId == id);

            eRynokData.SaveUsers();
            eRynokData.SaveSellerProfiles();
            eRynokData.SaveBuyerProfiles();

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

        private static void SaveProfile(
            int userId,
            UserRole role,
            string? phone,
            string? address)
        {
            if (role == UserRole.Seller)
            {
                eRynokData.SellerProfiles.Add(new SellerProfile
                {
                    UserId = userId,
                    Phone = phone!.Trim(),
                    Address = address!.Trim()
                });
            }
            else if (role == UserRole.Buyer)
            {
                eRynokData.BuyerProfiles.Add(new BuyerProfile
                {
                    UserId = userId,
                    Phone = phone!.Trim(),
                    Address = address!.Trim()
                });
            }

            eRynokData.SaveSellerProfiles();
            eRynokData.SaveBuyerProfiles();
        }

        private static void UpdateProfile(
            int userId,
            UserRole role,
            string? phone,
            string? address)
        {
            eRynokData.SellerProfiles.RemoveAll(p => p.UserId == userId);
            eRynokData.BuyerProfiles.RemoveAll(p => p.UserId == userId);

            SaveProfile(userId, role, phone, address);
        }

        private static UserResponseDto MapToDto(User user)
        {
            string? phone = null;
            string? address = null;

            if (user.Role == UserRole.Seller)
            {
                var profile = eRynokData.SellerProfiles
                    .FirstOrDefault(p => p.UserId == user.Id);

                phone = profile?.Phone;
                address = profile?.Address;
            }
            else if (user.Role == UserRole.Buyer)
            {
                var profile = eRynokData.BuyerProfiles
                    .FirstOrDefault(p => p.UserId == user.Id);

                phone = profile?.Phone;
                address = profile?.Address;
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
