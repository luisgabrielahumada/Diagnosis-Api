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
            services.AddDbContextPool<ApplicationDbContext>(options =>
                options.UseSqlServer(DbConfiguration.ConnectionString,
                                    sql => sql.EnableRetryOnFailure())
                       .EnableThreadSafetyChecks(false)
                       .EnableSensitiveDataLogging()
                       .EnableDetailedErrors()
                       .LogTo(Console.WriteLine, new[] {
                           DbLoggerCategory.Database.Command.Name,
                           DbLoggerCategory.Query.Name,
                           DbLoggerCategory.Infrastructure.Name
                        }, LogLevel.Information));


            services.AddScoped(typeof(IReadRepository<>), typeof(ReadRepository<>));
            services.AddScoped(typeof(IWriteRepository<>), typeof(WriteRepository<>));
            services.AddScoped<IDiagnosisRepository, DiagnosisRepository>();
            services.AddScoped<IPatientRepository, PatientRepository>();


        }
    }
}
