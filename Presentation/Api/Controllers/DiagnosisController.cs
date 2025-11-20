using Application.Dtos;
using Application.DTOs;
using Application.Services;
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

        [HttpPost("analyze/{diagnosisType=zombie}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(ApiResponse<PatientDiagnosisResponseDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> CreateDiagnosis([FromBody] PatientDiagnosisRequestDto patient, [FromRoute] string diagnosisType = "zombie")
        {

            var result = await _process.CreateDiagnosisAsync(diagnosisType, patient);

            return Response(new ApiResponse<PatientDiagnosisResponseDto>(result), onError: result.Data.Infected);
        }

        [HttpGet("stats")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<StatsResponseDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Stats([FromQuery] string diagnosisType = "zombie")
        {
            return Response(new ApiResponse<StatsResponseDto>(await _process.StatsAsync(diagnosisType)));
        }
    }
}