using Application.Dtos.Request;
using Application.Strategies.Interface;
using Shared.Response;

namespace Application.Strategies
{
    public class ZombieDiagnosisStrategy : IDiagnosisStrategy
    {
        public string TypeName =>"zombie";

        public Task<ServiceResponse<PatientDiagnosisResponseDto>> ExecuteAsync(PatientDiagnosisRequestDto dto)
        {
            throw new NotImplementedException();
        }
    }
}
