
using BaseLibrary.DTOs;
using BaseLibrary.Responses;
using ClientLibrary.Services.Interfaces;
using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;

namespace ClientLibrary.Helpers
{
    public class CustomHttpHandler : DelegatingHandler
    {
        private readonly LocalStorageProvider _localStorageProvider;
        private readonly IUserAccountService _accountService;

        public CustomHttpHandler(LocalStorageProvider localStorageProvider,
                                 IUserAccountService userAccountService )
        {
            _localStorageProvider = localStorageProvider;
            _accountService = userAccountService;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            bool loginUrl = request.RequestUri.AbsolutePath.Contains("login");
            bool registerUrl = request.RequestUri.AbsolutePath.Contains("register");
            bool refreshTokenUrl = request.RequestUri.AbsolutePath.Contains("refresh-token");

            // In this case we do not need to solve problem with refreshing token
            if (loginUrl || registerUrl || refreshTokenUrl)
            {
                return await base.SendAsync(request, cancellationToken);
            }

            // Send the request
            var result = await base.SendAsync(request, cancellationToken);
            if (result != null)
            {
                // If the status code is Unauthorized we need to refresh the token
                if (result.StatusCode == HttpStatusCode.Unauthorized)
                {
                    // Get token from local storage
                    var token = await _localStorageProvider.GetTokenAsync();

                    if(string.IsNullOrEmpty(token))
                    {
                        // If token is empty 
                        return result;
                    }

                    // Deserialize the token from json to a UserSessionDto object
                    var userSesion = Serializations.DeserializeJsonString<UserSessionDto>(token);

                    if(userSesion == null)
                    {
                        return result;
                    }

                    string currentToken = string.Empty;

                    // If the token is empty we try with the current token
                    if(request.Headers.Authorization == null ||
                        string.IsNullOrEmpty(request.Headers.Authorization.Parameter))
                    {
                        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", userSesion.Token);

                        // Send the request again
                        return await base.SendAsync(request, cancellationToken);
                    }

                    // If the token is not empty, we try to refresh the token
                    var loginResponse = await GetRefreshTokenAsync(userSesion.RefreshToken);

                    if (!loginResponse.Flag)
                    {
                        return await base.SendAsync(request, cancellationToken);
                    }

                    // Deserialization object to json string
                    // and save new token to local storage
                    var deserialization = Serializations.SerializeObj<UserSessionDto>(new UserSessionDto()
                    {
                        Token = loginResponse.Token,
                        RefreshToken = userSesion.RefreshToken
                    });
                    await _localStorageProvider.SetTokenAsync(deserialization);

                    // Set the new token to the request
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", loginResponse.Token);

                    return await base.SendAsync(request, cancellationToken);
                }
            }

            return result;
        }

        private async Task<LoginResponse> GetRefreshTokenAsync(string? refreshToken)
        {
            try
            {
                var result = await _accountService.RefreshTokenAsync(new RefreshTokenDto()
                {
                    RefreshToken = refreshToken
                });

                return result;

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return new LoginResponse(false, "Error while refreshing token");
            }
        }
    }
}
