using Application.Strategies.Interface;
using Microsoft.Extensions.DependencyInjection;
using Shared.Response;

namespace Application.Factory
{
    public interface IDiagnosisFactory
    {
        Task<ServiceResponse<IDiagnosisStrategy>> GetStrategyAsync(string diagnosisType);
    }
    public class DiagnosisFactory : IDiagnosisFactory
    {
        private readonly IServiceProvider _provider;

        public DiagnosisFactory(IServiceProvider provider)
        {
            _provider = provider;
        }

        public async Task<ServiceResponse<IDiagnosisStrategy>> GetStrategyAsync(string diagnosisType)
        {
            var response = new ServiceResponse<IDiagnosisStrategy>();

            try
            {
                var strategies = _provider.GetServices<IDiagnosisStrategy>();

                var strategy = strategies
                    .FirstOrDefault(s => s.TypeName.Equals(diagnosisType, StringComparison.OrdinalIgnoreCase));

                if (strategy is null)
                {
                    response.AddError($"Diagnosis type '{diagnosisType}' not supported.");
                    return response;
                }

                response.Data = strategy;
            }
            catch (Exception ex)
            {
                response.AddError(ex);

            }
            return response;
        }

    }

}
