using eRynok.DTOs.Users;
using eRynok.Models;

namespace eRynok.Services.Interfaces
{
    public interface IUserService
    {
        IEnumerable<UserResponseDto> GetUsers(
            string? search,
            UserRole? role);

        UserResponseDto? GetUser(int id);

        ServiceResult<UserResponseDto> CreateUser(CreateUserDto dto);

        ServiceResult<bool> UpdateUser(int id, UpdateUserDto dto);

        ServiceResult<bool> DeleteUser(int id);
    }
}
