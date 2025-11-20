using Application.DTOs.Validations;
using Application.Factory;
using Application.Services;
using Application.Strategies;
using Application.Strategies.Interface;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
namespace Application
{
    public static class ServiceCollectionExtension
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddValidatorsFromAssembly(typeof(DiagnosisRequestValidator).Assembly);
            services.AddScoped<IDiagnosisService, DiagnosisService>();
            services.AddScoped<IDiagnosisFactory, DiagnosisFactory>();
            services.AddScoped<IDiagnosisStrategy,ZombieDiagnosisStrategy>();
            services.AddScoped<IDiagnosisStrategy,CovidDiagnosisStrategy>();
            services.AddScoped<IDiagnosisStrategy,InfluenzaDiagnosisStrategy>();
            return services;
        }
    }
}
