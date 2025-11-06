using BankApp;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

// Main entry point for the BankApp.
// Sets up logging, components, and required services.

// Starts building the app and loads default settings. 
var builder = WebAssemblyHostBuilder.CreateDefault(args);

// Enables logging at the "Information" level. 
builder.Logging.SetMinimumLevel(LogLevel.Information); 

// Connects the main app to the web page.
builder.RootComponents.Add<App>("#app");

// Enables parts that update the page header. 
builder.RootComponents.Add<HeadOutlet>("head::after");

// Allows the app to automatically get the services it needs. 
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddBlazoredLocalStorage();
builder.Services.AddScoped<IStorageService, LocalStorageService>();

// Registers the PIN lock service with my fixed PIN code. 
builder.Services.AddScoped<IPinLockService>(sp =>
{
    var localStorage = sp.GetRequiredService<Blazored.LocalStorage.ILocalStorageService>();
    return new BankApp.Services.PinLockService(localStorage, "7788");

});

// Builds and runs the application.
await builder.Build().RunAsync();