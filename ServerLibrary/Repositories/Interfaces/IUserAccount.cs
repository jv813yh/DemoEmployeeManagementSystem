using BaseLibrary.DTOs;
using BaseLibrary.Responses;

namespace ServerLibrary.Repositories.Interfaces
{
    public interface IUserAccount
    {
        Task<GeneralResponse> CreateAccountAsync(RegisterDto registerDto);
        Task<LoginResponse> SingInAsync(LoginDto loginDto);
    }
}
