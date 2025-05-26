using Blazored.LocalStorage;
using FamilyTreeFrontend;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using Microsoft.AspNetCore.Components.Authorization;
using FamilyTreeFrontend.Helper;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddMudServices();


// Local storage

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthenticationStateProvider>();
builder.Services.AddAuthorizationCore();

builder.RootComponents.Add<App>("#app");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Register named HttpClient instances for both APIs
builder.Services.AddHttpClient("FamilyTreeAPI", client =>
{
    client.BaseAddress = new Uri("http://localhost:5199/api/");
});

builder.Services.AddHttpClient("LoginAPI", client =>
{
    client.BaseAddress = new Uri("http://localhost:5043/api/");
});

await builder.Build().RunAsync();