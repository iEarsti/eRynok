using eRynok.DTOs.Authorization;

namespace eRynok.Services.Interfaces
{
    public interface IAuthService
    {
        Task<ServiceResult<AuthResponseDto>> Register(
            RegisterDto dto);

        Task<ServiceResult<AuthResponseDto>> Login(
            LoginDto dto);
    }
}