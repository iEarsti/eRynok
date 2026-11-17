using eRynok.DTOs.Users;
using eRynok.Models;

namespace eRynok.Services.Interfaces
{
    public interface IUserService
    {
        Task<IEnumerable<UserResponseDto>> GetUsers(
            string? search,
            UserRole? role);

        Task<UserResponseDto?> GetUser(int id);

        Task<ServiceResult<UserResponseDto>> CreateUser(CreateUserDto dto);

        Task<ServiceResult<bool>> UpdateUser(int id, UpdateUserDto dto);

        Task<ServiceResult<bool>> DeleteUser(int id);
    }
}
