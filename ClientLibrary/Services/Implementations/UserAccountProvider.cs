using BaseLibrary.DTOs;
using BaseLibrary.Responses;
using ClientLibrary.Services.Interfaces;

namespace ClientLibrary.Services.Implementations
{
    public class UserAccountProvider : IUserAccountService
    {
        public Task<GeneralResponse> CreateAccountAsync(RegisterDto registerDto)
        {
            throw new NotImplementedException();
        }

        public Task<LoginResponse> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
        {
            throw new NotImplementedException();
        }

        public Task<LoginResponse> SignInAsync(LoginDto loginDto)
        {
            throw new NotImplementedException();
        }
    }
}
