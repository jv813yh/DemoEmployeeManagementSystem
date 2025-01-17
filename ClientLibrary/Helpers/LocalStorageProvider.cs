using Blazored.LocalStorage;

namespace ClientLibrary.Helpers
{
    public class LocalStorageProvider
    {
        private const string StorageKey = "authentication-token";
        private readonly ILocalStorageService _localStorageService;

        public LocalStorageProvider(ILocalStorageService localStorageService)
        {
            _localStorageService = localStorageService;
        }

        /// <summary>
        /// Get the token from local storage in json
        /// </summary>
        /// <returns></returns>
        public async Task<string?> GetTokenAsync()
            => await _localStorageService.GetItemAsStringAsync(StorageKey);

        /// <summary>
        /// Set the token in local storage in json
        /// </summary>
        /// <param name="jsonToken"></param>
        /// <returns></returns>
        public async Task SetTokenAsync(string jsonToken)
            => await _localStorageService.SetItemAsStringAsync(StorageKey, jsonToken);

        /// <summary>
        /// Remove the token from local storage
        /// </summary>
        /// <returns></returns>
        public async Task RemoveTokenAsync()
            => await _localStorageService.RemoveItemAsync(StorageKey);
    }
}
