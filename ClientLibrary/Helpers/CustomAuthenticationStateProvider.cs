using BaseLibrary.DTOs;
using Microsoft.AspNetCore.Components.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ClientLibrary.Helpers
{
    public class CustomAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly LocalStorageProvider _localStorageProvider;

        // Anonymous user (no registered user)
        private readonly ClaimsPrincipal _anonymousUser;
        public CustomAuthenticationStateProvider(LocalStorageProvider localStorageProvider)
        {
            _localStorageProvider = localStorageProvider;
            _anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var stringToken = await _localStorageProvider.GetTokenAsync();
            if (string.IsNullOrEmpty(stringToken))
            {
                return new AuthenticationState(_anonymousUser);
            }

            // Deserialize the token from json to a UserSessionDto object
            var deserializedToken = Serializations.DeserializeJsonString<UserSessionDto>(stringToken);
            if (deserializedToken == null)
            {
                return new AuthenticationState(_anonymousUser);
            }

            // Decrypt the token
            var userClaims = DecryptToken(deserializedToken.Token);
            if (userClaims == null)
            {
                return new AuthenticationState(_anonymousUser);
            }

            // Set the claims principal
            var claimsPrincipal = SetClaimPrincipal(userClaims);

            return new AuthenticationState(claimsPrincipal);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userSession"></param>
        /// <returns></returns>
        public async Task UpdateAuthenticationStateAsync(UserSessionDto userSession)
        {
            if(userSession == null)
            {
                return;
            }

            var claimsPrincipal = new ClaimsPrincipal();

            if (userSession.Token != null ||
               userSession.RefreshToken != null)
            {
                // Serialize the token to a json string
                var serialization = Serializations.SerializeObj<UserSessionDto>(userSession);
                // Save the token to local storage
                await _localStorageProvider.SetTokenAsync(serialization);
                var userClaims = DecryptToken(userSession.Token!);
                claimsPrincipal = SetClaimPrincipal(userClaims);
            }
            else
            {
                // Remove the token from local storage
                await _localStorageProvider.RemoveTokenAsync();
            }

            // Notify the authentication state has changed
            NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(claimsPrincipal)));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userClaims"></param>
        /// <returns></returns>
        private ClaimsPrincipal SetClaimPrincipal(CustomerUserClaimsDto userClaims)
        {
            // Create a new list of claims
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userClaims.Id),
                new Claim(ClaimTypes.Name, userClaims.Name),
                new Claim(ClaimTypes.Email, userClaims.Email),
                new Claim(ClaimTypes.Role, userClaims.Role)
            };

            // Create a new ClaimsIdentity with the claims
            var identity = new ClaimsIdentity(claims, "JwtAuth");

            // return a new ClaimsPrincipal with the identity
            return new ClaimsPrincipal(identity);
        }

        /// <summary>
        /// Decrypt the token and return the claims 
        /// </summary>
        /// <param name="jwtToken"></param>
        /// <returns></returns>
        private CustomerUserClaimsDto DecryptToken(string jwtToken)
        {
            if(string.IsNullOrEmpty(jwtToken))
            {
                return new CustomerUserClaimsDto();
            }

            // Handler for the JWT token
            var handler = new JwtSecurityTokenHandler();

            // Read the token
            var token = handler.ReadJwtToken(jwtToken);

            // Get the claims from the token
            var userId = token.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier);
            var name = token.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Name);
            var email = token.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Email);
            var role = token.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.Role);

            // return new ClaimsPrincipal with the claims from the token
            return new CustomerUserClaimsDto
            {
                Id = userId!.Value,
                Name = name!.Value,
                Email = email!.Value,
                Role = role!.Value
            };
        }
    }
}
