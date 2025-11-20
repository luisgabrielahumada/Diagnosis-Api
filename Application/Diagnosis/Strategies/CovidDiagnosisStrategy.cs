using Application.Dtos;
using Application.Strategies.Interface;
using Shared;
using Shared.Response;

namespace Application.Strategies
{
    public class CovidDiagnosisStrategy : IDiagnosisStrategy
    {
        public string TypeName => Constants.DiagnosisType.Covid;

        public Task<ServiceResponse<PatientDiagnosisResponseDto>> ExecuteAsync(PatientDiagnosisRequestDto input)
        {
            throw new NotImplementedException();
        }
    }
}
