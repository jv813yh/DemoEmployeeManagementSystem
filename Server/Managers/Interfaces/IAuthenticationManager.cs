using BaseLibrary.DTOs;
using BaseLibrary.Responses;

namespace Server.Managers.Interfaces
{
    public interface IAuthenticationManager
    {
        Task<GeneralResponse> CreateAccountByManagerAsync(RegisterDto registerDto);
        Task<LoginResponse> SingInByManagerAsync(LoginDto loginDto);
        Task<LoginResponse> RefreshTokenByManagerAsync(RefreshTokenDto refreshTokenDto);
    }
}
