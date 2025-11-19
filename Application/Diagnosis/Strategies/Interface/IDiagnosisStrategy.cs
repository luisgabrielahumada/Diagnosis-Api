using Application.Dtos.Request;
using Shared.Response;
namespace Application.Strategies.Interface
{
    public interface IDiagnosisStrategy
    {
        string TypeName { get; }
        Task<ServiceResponse<PatientDiagnosisResponseDto>> ExecuteAsync(PatientDiagnosisRequestDto dto);
    }
}
