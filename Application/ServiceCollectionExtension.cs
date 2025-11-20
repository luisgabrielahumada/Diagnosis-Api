using Application.Factory;
using Application.Services;
using Application.Strategies;
using Application.Strategies.Interface;
using Microsoft.Extensions.DependencyInjection;
namespace Application
{
    public static class ServiceCollectionExtension
    {
        public static void AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IDiagnosisService, DiagnosisService>();
            services.AddScoped<IDiagnosisFactory, DiagnosisFactory>();
            services.AddScoped<IDiagnosisStrategy,ZombieDiagnosisStrategy>();
            services.AddScoped<IDiagnosisStrategy,CovidDiagnosisStrategy>();
            services.AddScoped<IDiagnosisStrategy,InfluenzaDiagnosisStrategy>();
        }
    }
}
