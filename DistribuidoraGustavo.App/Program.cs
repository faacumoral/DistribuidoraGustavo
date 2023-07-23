using DistribuidoraGustavo.App;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using DistribuidoraGustavo.App.Http;
using Blazored.SessionStorage;
using Tewr.Blazor.FileReader;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

//builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

var apiUrl = builder.Configuration["ApiUrl"];
builder.Services.AddHttpClient<ApiClient>(
    client => client.BaseAddress = new Uri(builder.Configuration["ApiUrl"]));

builder.Services.AddFileReaderService(o => o.UseWasmSharedBuffer = true);

builder.Services.AddBlazoredSessionStorage();

await builder.Build().RunAsync();
