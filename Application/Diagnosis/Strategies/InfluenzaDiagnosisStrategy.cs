using Application.Dtos;
using Application.Strategies.Interface;
using Shared;
using Shared.Response;

namespace Application.Strategies
{
    public class InfluenzaDiagnosisStrategy : IDiagnosisStrategy
    {
        public string TypeName => Constants.DiagnosisType.Influenza;

        public Task<ServiceResponse<PatientDiagnosisResponseDto>> ExecuteAsync(PatientDiagnosisRequestDto input)
        {
            throw new NotImplementedException();
        }
    }
}
