using Application.Dtos.Request;
using Application.Services.Diagnosis;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Response;
using Web.Api.Code;

namespace Web.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Area(nameof(Controllers))]
    [AllowAnonymous]
    public class DiagnosisController : BaseApiController
    {
        private readonly IDiagnosisService _process;

        public DiagnosisController(IDiagnosisService process)
        {
            _process = process;
        }

        [HttpPost("analyze/{diagnosisType}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PatientDiagnosisResponseDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateDiagnosis([FromRoute] string diagnosisType, [FromBody] PatientDiagnosisRequestDto patient)
        {
            return Response(new ApiResponse<PatientDiagnosisResponseDto>(await _process.CreateDiagnosisAsync(diagnosisType, patient)));
        }
    }
}