using DistribuidoraGustavo.App;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using DistribuidoraGustavo.App.Http;
using Blazored.SessionStorage;
using DistribuidoraGustavo.App.Services.Toast;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

//builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddHttpClient<ApiClient>(
    client => client.BaseAddress = new Uri(builder.Configuration["ApiUrl"]));

builder.Services.AddBlazoredSessionStorage();
builder.Services.AddScoped<ToastService>();

await builder.Build().RunAsync();
