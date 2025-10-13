using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace SkinCareSystem.Repositories.DBContext
{
    public class SkinCareSystemDbContextFactory : IDesignTimeDbContextFactory<SkinCareSystemDbContext>
    {
        public SkinCareSystemDbContext CreateDbContext(string[] args)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true)
                .AddJsonFile($"appsettings.{environment}.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var connectionString = configuration.GetConnectionString("DefaultConnection")
                                   ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

            var optionsBuilder = new DbContextOptionsBuilder<SkinCareSystemDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new SkinCareSystemDbContext(optionsBuilder.Options);
        }
    }
}
