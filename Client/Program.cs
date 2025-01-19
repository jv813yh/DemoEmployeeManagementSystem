using Blazored.LocalStorage;
using Client;
using ClientLibrary.Helpers;
using ClientLibrary.Services.Implementations.UserAccountServices;
using ClientLibrary.Services.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("https://localhost:7206") });

// For authorization
builder.Services.AddAuthorizationCore();
// For local storage
builder.Services.AddBlazoredLocalStorage();

// Services
builder.Services.AddScoped<HttpClientFactory>();
builder.Services.AddScoped<LocalStorageProvider>();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddScoped<IUserAccountService, UserAccountProvider>();

await builder.Build().RunAsync();
