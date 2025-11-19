using Infrastructure.Interfaces;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared;
namespace Infrastructure
{
    public static class ServiceCollectionExtension
    {
        public static void AddInfrastructureServices(this IServiceCollection services)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(DbConfiguration.ConnectionString)
                       .EnableSensitiveDataLogging() 
                       .EnableDetailedErrors()  
                       .LogTo(Console.WriteLine, new[] {
                           DbLoggerCategory.Database.Command.Name,
                           DbLoggerCategory.Query.Name,
                           DbLoggerCategory.Infrastructure.Name
                        }, LogLevel.Information)); 

            services.AddScoped(typeof(IReadRepository<>), typeof(ReadRepository<>));
            services.AddScoped(typeof(IWriteRepository<>), typeof(WriteRepository<>));

        }
    }
}
