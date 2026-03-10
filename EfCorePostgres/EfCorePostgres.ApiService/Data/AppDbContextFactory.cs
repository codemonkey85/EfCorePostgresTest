using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace EfCorePostgres.ApiService.Data;

// Used by EF Core tools (dotnet ef migrations add, etc.) at design time.
public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
        optionsBuilder.UseNpgsql("Host=localhost;Database=efcorepostgresdb;Username=postgres;Password=postgres");
        return new AppDbContext(optionsBuilder.Options);
    }
}
