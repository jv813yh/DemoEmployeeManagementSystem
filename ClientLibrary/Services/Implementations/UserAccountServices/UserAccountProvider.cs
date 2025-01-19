using BaseLibrary.DTOs;
using BaseLibrary.Responses;
using ClientLibrary.Helpers;
using ClientLibrary.Services.Interfaces;
using System.Diagnostics;
using System.Net.Http.Json;

namespace ClientLibrary.Services.Implementations.UserAccountServices
{
    public class UserAccountProvider : IUserAccountService
    {
        private const string AuthUrl = "/api/authentication";
        private readonly HttpClientFactory _httpClientFactory;
        public UserAccountProvider(HttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Create an account with the registerDto
        /// </summary>
        /// <param name="registerDto"></param>
        /// <returns></returns>
        public async Task<GeneralResponse> CreateAccountAsync(RegisterDto registerDto)
        {
            if(registerDto == null)
            {
                return new GeneralResponse(false, "Model cannot be null.");
            }

            try
            {
                // Get the public HttpClient
                var publicHttpClient = _httpClientFactory.GetPublicHttpClient();
                // Post the registerDto to the server
                var response = await publicHttpClient.PostAsJsonAsync(AuthUrl + "/register", registerDto);

                if(!response.IsSuccessStatusCode)
                {
                    return new GeneralResponse(false, "Account has not created successfully");
                }

                // Get the result from the response in json
                var result =  await response.Content.ReadFromJsonAsync<GeneralResponse>();

                if(result == null)
                {
                    return new GeneralResponse(false, "Error while creating account");
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return new GeneralResponse(false, "Error while creating account");
            }
        }

        public Task<LoginResponse> RefreshTokenAsync(RefreshTokenDto refreshTokenDto)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Sign in with the loginDto
        /// </summary>
        /// <param name="loginDto"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<LoginResponse> SignInAsync(LoginDto loginDto)
        {
            if(loginDto == null)
            {
                return new LoginResponse(false, "Model cannot be null.");
            }

            try
            {
                // Get the public HttpClient
                var publicHttpClient = _httpClientFactory.GetPublicHttpClient();
                // Post the loginDto to the server
                var response = await publicHttpClient.PostAsJsonAsync(AuthUrl + "/login", loginDto);

                if (!response.IsSuccessStatusCode)
                {
                    return new LoginResponse(false, "Login failed");
                }

                // Get the token from the response
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                if(result == null)
                {
                    return new LoginResponse(false, "Error during login into system");
                }

                return result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return new LoginResponse(false, "Error during login into system");
            }
        }
    }
}
