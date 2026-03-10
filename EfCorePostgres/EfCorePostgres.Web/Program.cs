using EfCorePostgres.Web;
using EfCorePostgres.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

builder.Services.AddOutputCache();

// Named HTTP client used to proxy YouTube links calls to the API service.
builder.Services.AddHttpClient("apiservice", client =>
{
    client.BaseAddress = new("https+http://apiservice");
});

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
    app.UseHttpsRedirection();
}
app.UseAntiforgery();
app.UseOutputCache();
app.MapStaticAssets();

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(EfCorePostgres.Web.Client.YouTubeLinksClient).Assembly);

// Proxy endpoints: forward YouTube links requests from WASM client to the API service.
app.MapGet("/api/youtube-links", async (IHttpClientFactory factory) =>
{
    using var client = factory.CreateClient("apiservice");
    var response = await client.GetAsync("/youtube-links");
    var content = await response.Content.ReadAsStringAsync();
    return Results.Content(content, "application/json", statusCode: (int)response.StatusCode);
});

app.MapGet("/api/youtube-links/{key}", async (string key, IHttpClientFactory factory) =>
{
    using var client = factory.CreateClient("apiservice");
    var response = await client.GetAsync($"/youtube-links/{Uri.EscapeDataString(key)}");
    var content = await response.Content.ReadAsStringAsync();
    return Results.Content(content, "application/json", statusCode: (int)response.StatusCode);
});

app.MapPost("/api/youtube-links", async (HttpContext ctx, IHttpClientFactory factory) =>
{
    using var client = factory.CreateClient("apiservice");
    var body = await new StreamReader(ctx.Request.Body).ReadToEndAsync();
    var response = await client.PostAsync("/youtube-links",
        new StringContent(body, System.Text.Encoding.UTF8, "application/json"));
    var content = await response.Content.ReadAsStringAsync();
    return Results.Content(content, "application/json", statusCode: (int)response.StatusCode);
});

app.MapPut("/api/youtube-links/{key}", async (string key, HttpContext ctx, IHttpClientFactory factory) =>
{
    using var client = factory.CreateClient("apiservice");
    var body = await new StreamReader(ctx.Request.Body).ReadToEndAsync();
    var response = await client.PutAsync($"/youtube-links/{Uri.EscapeDataString(key)}",
        new StringContent(body, System.Text.Encoding.UTF8, "application/json"));
    var content = await response.Content.ReadAsStringAsync();
    return Results.Content(content, "application/json", statusCode: (int)response.StatusCode);
});

app.MapDelete("/api/youtube-links/{key}", async (string key, IHttpClientFactory factory) =>
{
    using var client = factory.CreateClient("apiservice");
    var response = await client.DeleteAsync($"/youtube-links/{Uri.EscapeDataString(key)}");
    return Results.StatusCode((int)response.StatusCode);
});

app.MapDefaultEndpoints();

app.Run();
