using Application.Dtos.Request;
using Application.Strategies.Interface;
using Shared.Response;

namespace Application.Strategies
{
    public class InfluenzaDiagnosisStrategy : IDiagnosisStrategy
    {
        public string TypeName => "influenza";

        public Task<ServiceResponse<PatientDiagnosisResponseDto>> ExecuteAsync(PatientDiagnosisRequestDto dto)
        {
            throw new NotImplementedException();
        }
    }
}
