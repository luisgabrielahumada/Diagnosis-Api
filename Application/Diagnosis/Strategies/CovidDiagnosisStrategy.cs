using Application.Dtos.Request;
using Application.Strategies.Interface;
using Shared.Response;

namespace Application.Strategies
{
    public class CovidDiagnosisStrategy : IDiagnosisStrategy
    {
        public string TypeName => "covid";

        public Task<ServiceResponse<PatientDiagnosisResponseDto>> ExecuteAsync(PatientDiagnosisRequestDto dto)
        {
            throw new NotImplementedException();
        }
    }
}
