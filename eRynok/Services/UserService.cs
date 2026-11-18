using eRynok.Models;
using eRynok.DTOs.Users;
using eRynok.Services.Interfaces;
using eRynok.Repositories.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace eRynok.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public UserService(
            IUserRepository userRepository,
            UserManager<ApplicationUser> userManager)
        {
            _userRepository = userRepository;
            _userManager = userManager;
        }

        public async Task<IEnumerable<UserResponseDto>> GetUsers(
            string? search,
            UserRole? role)
        {
            var users = await _userRepository.GetUsers(
                search?.Trim(),
                role?.ToString());

            var result = new List<UserResponseDto>();

            foreach (var user in users)
            {
                result.Add(await MapToDto(user));
            }

            return result;
        }

        public async Task<UserResponseDto?> GetUser(int id)
        {
            var user = await _userRepository.GetUser(id);

            if (user == null)
            {
                return null;
            }

            return await MapToDto(user);
        }

        public async Task<ServiceResult<UserResponseDto>> CreateUser(
            CreateUserDto dto)
        {
            var email = dto.Email.Trim();

            var existingUser = await _userManager.FindByEmailAsync(email);

            if (existingUser != null)
            {
                return ServiceResult<UserResponseDto>.Failure(
                    ServiceError.Conflict,
                    "A user with this email already exists.");
            }

            var validationError = ValidateProfile(
                dto.Role,
                dto.Phone,
                dto.Address);

            if (validationError != null)
            {
                return ServiceResult<UserResponseDto>.Failure(
                    ServiceError.InvalidReference,
                    validationError);
            }

            var user = new ApplicationUser
            {
                Name = dto.Name.Trim(),
                UserName = email,
                Email = email
            };

            UpdateProfile(
                user,
                dto.Role,
                dto.Phone,
                dto.Address);

            var createResult = await _userManager.CreateAsync(
                user,
                dto.Password);

            if (!createResult.Succeeded)
            {
                var errors = string.Join(
                    " ",
                    createResult.Errors.Select(error => error.Description));

                return ServiceResult<UserResponseDto>.Failure(
                    ServiceError.Conflict,
                    errors);
            }

            var roleResult = await _userManager.AddToRoleAsync(
                user,
                dto.Role.ToString());

            if (!roleResult.Succeeded)
            {
                await _userManager.DeleteAsync(user);

                var errors = string.Join(
                    " ",
                    roleResult.Errors.Select(error => error.Description));

                return ServiceResult<UserResponseDto>.Failure(
                    ServiceError.Conflict,
                    errors);
            }

            return ServiceResult<UserResponseDto>.Success(
                await MapToDto(user));
        }

        public async Task<ServiceResult<bool>> UpdateUser(
            int id,
            UpdateUserDto dto)
        {
            var user = await _userRepository.GetUser(id);

            if (user == null)
            {
                return ServiceResult<bool>.Failure(
                    ServiceError.NotFound,
                    "User not found.");
            }

            var email = dto.Email.Trim();

            var existingUser = await _userManager.FindByEmailAsync(email);

            if (existingUser != null && existingUser.Id != id)
            {
                return ServiceResult<bool>.Failure(
                    ServiceError.Conflict,
                    "Another user with this email already exists.");
            }

            var currentRoles = await _userManager.GetRolesAsync(user);

            var currentRole = currentRoles.FirstOrDefault();

            if (string.Equals(currentRole, UserRole.Seller.ToString(), StringComparison.OrdinalIgnoreCase)
                && dto.Role != UserRole.Seller
                && await _userRepository.HasProducts(id))
            {
                return ServiceResult<bool>.Failure(
                    ServiceError.Conflict,
                    "Cannot change the role of a seller who has products.");
            }

            var validationError = ValidateProfile(
                dto.Role,
                dto.Phone,
                dto.Address);

            if (validationError != null)
            {
                return ServiceResult<bool>.Failure(
                    ServiceError.InvalidReference,
                    validationError);
            }

            user.Name = dto.Name.Trim();
            user.Email = email;
            user.UserName = email;

            UpdateProfile(
                user,
                dto.Role,
                dto.Phone,
                dto.Address);

            var updateResult = await _userManager.UpdateAsync(user);

            if (!updateResult.Succeeded)
            {
                var errors = string.Join(
                    " ",
                    updateResult.Errors.Select(error => error.Description));

                return ServiceResult<bool>.Failure(
                    ServiceError.Conflict,
                    errors);
            }

            if (!string.Equals(currentRole, dto.Role.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                if (currentRoles.Any())
                {
                    var removeResult = await _userManager.RemoveFromRolesAsync(
                        user,
                        currentRoles);

                    if (!removeResult.Succeeded)
                    {
                        return ServiceResult<bool>.Failure(
                            ServiceError.Conflict,
                            "Failed to update user roles.");
                    }
                }

                var addResult = await _userManager.AddToRoleAsync(
                    user,
                    dto.Role.ToString());

                if (!addResult.Succeeded)
                {
                    return ServiceResult<bool>.Failure(
                        ServiceError.Conflict,
                        "Failed to update user roles.");
                }
            }

            return ServiceResult<bool>.Success(true);
        }

        public async Task<ServiceResult<bool>> DeleteUser(int id)
        {
            var user = await _userRepository.GetUser(id);

            if (user == null)
            {
                return ServiceResult<bool>.Failure(
                    ServiceError.NotFound,
                    "User not found.");
            }

            if (await _userRepository.HasProducts(id))
            {
                return ServiceResult<bool>.Failure(
                    ServiceError.Conflict,
                    "Cannot delete a user who owns products.");
            }

            if (await _userRepository.HasOrders(id))
            {
                return ServiceResult<bool>.Failure(
                    ServiceError.Conflict,
                    "Cannot delete a user who has placed orders.");
            }

            var deleteResult = await _userManager.DeleteAsync(user);

            if (!deleteResult.Succeeded)
            {
                var errors = string.Join(
                    " ",
                    deleteResult.Errors.Select(error => error.Description));

                return ServiceResult<bool>.Failure(
                    ServiceError.Conflict,
                    errors);
            }

            return ServiceResult<bool>.Success(true);
        }

        private string? ValidateProfile(
            UserRole role,
            string? phone,
            string? address)
        {
            if (role == UserRole.Admin)
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(phone))
            {
                return "Phone is required for sellers and buyers.";
            }

            if (string.IsNullOrWhiteSpace(address))
            {
                return "Address is required for sellers and buyers.";
            }

            return null;
        }

        private void UpdateProfile(
            ApplicationUser user,
            UserRole role,
            string? phone,
            string? address)
        {
            switch (role)
            {
                case UserRole.Seller:
                    user.BuyerProfile = null;

                    user.SellerProfile ??= new SellerProfile();

                    user.SellerProfile.Phone = phone!.Trim();
                    user.SellerProfile.Address = address!.Trim();

                    break;

                case UserRole.Buyer:
                    user.SellerProfile = null;

                    user.BuyerProfile ??= new BuyerProfile();

                    user.BuyerProfile.Phone = phone!.Trim();
                    user.BuyerProfile.Address = address!.Trim();

                    break;

                case UserRole.Admin:
                    user.SellerProfile = null;
                    user.BuyerProfile = null;

                    break;
            }
        }

        private async Task<UserResponseDto> MapToDto(
            ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            Enum.TryParse<UserRole>(
            roles.FirstOrDefault(),
            true,
            out var role);

            string? phone = null;
            string? address = null;

            switch (role)
            {
                case UserRole.Seller:
                    phone = user.SellerProfile?.Phone;
                    address = user.SellerProfile?.Address;
                    break;

                case UserRole.Buyer:
                    phone = user.BuyerProfile?.Phone;
                    address = user.BuyerProfile?.Address;
                    break;
            }

            return new UserResponseDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email ?? string.Empty,
                Role = role,
                Phone = phone,
                Address = address
            };
        }
    }
}
