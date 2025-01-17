using BaseLibrary.DTOs;
using BaseLibrary.Responses;

namespace ClientLibrary.Services.Interfaces
{
    public interface IUserAccountService
    {
        Task<GeneralResponse> CreateAccountAsync(RegisterDto registerDto);
        Task<LoginResponse> SignInAsync(LoginDto loginDto);
        Task<LoginResponse> RefreshTokenAsync(RefreshTokenDto refreshTokenDto);
    }
}
