using Blazored.LocalStorage;
using DurableDice.GameClient;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

builder.Services.AddBlazoredLocalStorage();

builder.Services.AddSingleton(new HttpClient());

await builder.Build().RunAsync();
