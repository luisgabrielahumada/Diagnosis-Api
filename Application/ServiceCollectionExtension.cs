using Application.Factory;
using Application.Strategies;
using Microsoft.Extensions.DependencyInjection;
namespace Application
{
    public static class ServiceCollectionExtension
    {
        public static void AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IDiagnosisFactory, DiagnosisFactory>();
            services.AddScoped<ZombieDiagnosisStrategy>();
            services.AddScoped<CovidDiagnosisStrategy>();
            services.AddScoped<InfluenzaDiagnosisStrategy>();
        }
    }
}
