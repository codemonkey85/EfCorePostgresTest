using EfCorePostgres.ApiService.Data;
using EfCorePostgres.ApiService.Models;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add service defaults & Aspire client integrations.
builder.AddServiceDefaults();

// Add services to the container.
builder.Services.AddProblemDetails();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add EF Core with PostgreSQL via Aspire.
builder.AddNpgsqlDbContext<AppDbContext>("efcorepostgresdb");

var app = builder.Build();

// Apply pending migrations on startup.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

// Configure the HTTP request pipeline.
app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/", () => "API service is running.");

// YouTube Links endpoints
app.MapGet("/youtube-links", async (AppDbContext db) =>
    await db.YouTubeLinks.ToListAsync())
    .WithName("GetYouTubeLinks");

app.MapGet("/youtube-links/{key}", async (string key, AppDbContext db) =>
    await db.YouTubeLinks.FirstOrDefaultAsync(l => l.Key == key)
        is YouTubeLink link
            ? Results.Ok(link)
            : Results.NotFound())
    .WithName("GetYouTubeLinkByKey");

app.MapPost("/youtube-links", async (YouTubeLinkDto dto, AppDbContext db) =>
{
    var link = new YouTubeLink { Key = dto.Key, Url = dto.Url };
    db.YouTubeLinks.Add(link);
    await db.SaveChangesAsync();
    return Results.Created($"/youtube-links/{link.Key}", link);
})
.WithName("CreateYouTubeLink");

app.MapPut("/youtube-links/{key}", async (string key, YouTubeLinkDto dto, AppDbContext db) =>
{
    var link = await db.YouTubeLinks.FirstOrDefaultAsync(l => l.Key == key);
    if (link is null) return Results.NotFound();

    link.Key = dto.Key;
    link.Url = dto.Url;
    await db.SaveChangesAsync();
    return Results.Ok(link);
})
.WithName("UpdateYouTubeLink");

app.MapDelete("/youtube-links/{key}", async (string key, AppDbContext db) =>
{
    var link = await db.YouTubeLinks.FirstOrDefaultAsync(l => l.Key == key);
    if (link is null) return Results.NotFound();

    db.YouTubeLinks.Remove(link);
    await db.SaveChangesAsync();
    return Results.NoContent();
})
.WithName("DeleteYouTubeLink");

app.MapDefaultEndpoints();

app.Run();

record YouTubeLinkDto(string Key, string Url);
