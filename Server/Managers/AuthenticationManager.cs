using BaseLibrary.DTOs;
using BaseLibrary.Responses;
using Server.Managers.Interfaces;
using ServerLibrary.Repositories.Interfaces;

namespace Server.Managers
{
    public class AuthenticationManager : IAuthenticationManager
    {
        private readonly IUserAccount _userAccount;

        public AuthenticationManager(IUserAccount userAccount)
        {
            _userAccount = userAccount;
        }

        /// <summary>
        /// Async method to create an account.
        /// Is possible create user account or admin (admin just can be created once)
        /// </summary>
        /// <param name="registerDto"></param>
        /// <returns></returns>
        public Task<GeneralResponse> CreateAccountByManagerAsync(RegisterDto registerDto)
         => _userAccount.CreateAccountAsync(registerDto);

        /// <summary>
        /// Async method to refresh token.
        /// </summary>
        /// <param name="refreshTokenDto"></param>
        /// <returns></returns>
        public Task<LoginResponse> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
         => _userAccount.RefreshTokenAsync(refreshTokenDto);

        /// <summary>
        /// Async method to login.
        /// </summary>
        /// <param name="loginDto"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public Task<LoginResponse> SingInByManagerAsync(LoginDto loginDto)
         => _userAccount.SingInAsync(loginDto);
    }
}
