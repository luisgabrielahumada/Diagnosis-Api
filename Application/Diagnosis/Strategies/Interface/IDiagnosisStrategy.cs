using Application.Dtos;
using Shared.Response;
namespace Application.Strategies.Interface
{
    public interface IDiagnosisStrategy
    {
        string TypeName { get; }
        Task<ServiceResponse<PatientDiagnosisResponseDto>> ExecuteAsync(PatientDiagnosisRequestDto input);
    }
}
