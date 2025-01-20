using BaseLibrary.DTOs;
using System.Net.Http.Headers;

namespace ClientLibrary.Helpers
{
    public class HttpClientFactory
    {
        private const string HeaderKey = "Authorization";
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly LocalStorageProvider _localStorageProvider;

        public HttpClientFactory(IHttpClientFactory httpClientFactory, 
                             LocalStorageProvider localStorageProvider)
        {
            _httpClientFactory = httpClientFactory;
            _localStorageProvider = localStorageProvider;
        }

        /// <summary>
        /// Get private HttpClient with Authorization header name SystemApiClient
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception"></exception>
        public async Task<HttpClient> GetPrivateHttpClientAsync()
        {
            // Create a new HttpClient instance
            var httpClient = _httpClientFactory.CreateClient(Constants.HttpClientApiName);

            // Get the token from local storage
            var token = await _localStorageProvider.GetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                throw new Exception("Token not found in local storage");
            }

            // Deserialize the token from json to a UserSessionDto object
            var deserializationToken = Serializations.DeserializeJsonString<UserSessionDto>(token);
            if (deserializationToken == null)
            {
                throw new Exception("Token deserialization failed");
            }

            // Add the token to the Authorization header
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", deserializationToken.Token);

            return httpClient;
        }

        /// <summary>
        /// Get public HttpClient without Authorization header name SystemApiClient
        /// </summary>
        /// <returns></returns>
        public HttpClient GetPublicHttpClient()
        {
            var httpClient = _httpClientFactory.CreateClient(Constants.HttpClientApiName);
            httpClient.DefaultRequestHeaders.Remove(HeaderKey);

            return httpClient;
        }
    }
}
