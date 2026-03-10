using EfCorePostgres.ApiService.Models;
using Microsoft.EntityFrameworkCore;

namespace EfCorePostgres.ApiService.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<YouTubeLink> YouTubeLinks => Set<YouTubeLink>();
}
