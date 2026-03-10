using EfCorePostgres.Web.Client;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// HttpClient base address points back to the Web server (same origin),
// which proxies API calls to the ApiService using Aspire service discovery.
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
});

builder.Services.AddScoped<YouTubeLinksClient>();

await builder.Build().RunAsync();
