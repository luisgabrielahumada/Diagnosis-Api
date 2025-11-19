using Application.Dtos.Request;
using Application.Factory;
using Shared.Response;
namespace Application.Services.Diagnosis
{
    public interface IDiagnosisService
    {
        Task<ServiceResponse<PatientDiagnosisResponseDto>> CreateDiagnosisAsync(string diagnosisType, PatientDiagnosisRequestDto patient);
    }

    public class DiagnosisService : IDiagnosisService
    {
        private readonly IDiagnosisFactory _factory;

        public DiagnosisService(IDiagnosisFactory factory)
        {
            _factory = factory;
        }

        public async Task<ServiceResponse<PatientDiagnosisResponseDto>> CreateDiagnosisAsync(string diagnosisType, PatientDiagnosisRequestDto patient)
        {
            var sr = new ServiceResponse<PatientDiagnosisResponseDto>();
            try
            {
                var strategyResp = await _factory.GetStrategyAsync(diagnosisType);
                if (!strategyResp.Status)
                {
                    sr.AddErrors(strategyResp.Errors);
                    return sr;
                }

                var strategy = strategyResp.Data;
                var resp = await strategy.ExecuteAsync(patient);
                sr.Data = new PatientDiagnosisResponseDto
                {

                };
            }
            catch (Exception ex)
            {
                sr.AddError(ex);
            }
            return sr;
        }
    }


}
