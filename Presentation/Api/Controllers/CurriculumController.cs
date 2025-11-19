using Application.Dto;
using Application.Dto.Filters;
using Application.Dto.Filters.Curricunlum;
using Application.Services.Curriculum;
using Application.Services.Curriculum.Models;
using Infrastructure.Dto;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Pagination;
using Shared.Response;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Area(nameof(Controllers))]
    //[Authorize]
    [AllowAnonymous]
    public class CurriculumController : BaseApiController
    {
        private readonly IAcademicUnitService _unit;
        private readonly IAcademicPerformanceService _performance;
        private readonly IAcademicObjectiveService _objective;
        private readonly IAcademicEssentialKnowledgeService _knowledge;
        private readonly IAcademicMethodologyService _metodology;
        private readonly ICurriculumFrameworkService _mcc;

        public CurriculumController(IAcademicUnitService unit, IAcademicPerformanceService performance,
            IAcademicObjectiveService objective, IAcademicEssentialKnowledgeService knowledge,
            IAcademicMethodologyService metodology,
            ICurriculumFrameworkService mcc)
        {
            _unit = unit;
            _performance = performance;
            _objective = objective;
            _knowledge = knowledge;
            _metodology = metodology;
            _mcc = mcc;
        }


        [HttpGet("all")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<MainCurriculumFrameworkInboxDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAllUnits([FromQuery] int pageIndex, [FromQuery] int pageSize, [FromQuery] MainCurriculumFrameworkInfoDto filter = null)
        {
            return Response(new ApiResponse<MainCurriculumFrameworkInboxDto>(await _mcc.GetAllAsync(pageIndex, pageSize, filter)));
        }


        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<MainCurriculumFrameworkDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id, [FromQuery] MainCurriculumFrameworkInfoDto filter = null)
        {
            return Response(new ApiResponse<MainCurriculumFrameworkDto>(await _mcc.GetByIdAsync(id, filter)));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<MainCurriculumFrameworkDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Create(MainCurriculumFrameworkDto data)
        {
            return Response(new ApiResponse<MainCurriculumFrameworkDto>(await _mcc.SaveAsync(Guid.Empty, data)));
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<MainCurriculumFrameworkDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id,  MainCurriculumFrameworkDto data)
        {
            return Response(new ApiResponse<MainCurriculumFrameworkDto>(await _mcc.SaveAsync(id, data)));
        }


        [HttpGet("{id}/export")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetExport(Guid id, [FromQuery] MainCurriculumFrameworkInfoDto filter = null)
        {
            var result = await _mcc.GenerateAsync(id, filter);
            HttpContext.Response.Headers.Append("Access-Control-Expose-Headers", "Content-Disposition, X-Filename");
            HttpContext.Response.Headers.Append("X-Filename", result.Data.FileName);

            return File(
                fileContents: result.Data.Content,
                contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                fileDownloadName: result.Data.FileName
            );
        }

        [HttpPost("{id}/import")]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ImportSummary>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Import(Guid id, [FromForm] ImportMccRequest file)
        {
            return Response(new ApiResponse<ImportSummary>(await _mcc.ImportAsync(id, file)));
        }

        #region Unit
        [HttpGet("list-units")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PaginatedList<AcademicUnitDto>>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAllUnits([FromQuery] int pageIndex, [FromQuery] int pageSize, [FromQuery] AcademicUnitInfoDto filter = null)
        {
            return Response(new ApiResponse<PaginatedList<AcademicUnitDto>>(await _unit.GetAllAsync(pageIndex, pageSize, filter)));
        }

        [HttpGet("unit/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<AcademicUnitDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUnitById(Guid id)
        {
            return Response(new ApiResponse<AcademicUnitDto>(await _unit.GetByIdAsync(id)));
        }

        [HttpPost("unit")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddUnit(AcademicUnitDto data)
        {
            return Response(new ApiResponse(await _unit.SaveAsync(Guid.Empty, data)));
        }

        [HttpPut("unit/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateUnit(Guid id, AcademicUnitDto data)
        {
            return Response(new ApiResponse(await _unit.SaveAsync(id, data)));
        }

        [HttpDelete("unit/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUnit(Guid id)
        {
            return Response(new ApiResponse(await _unit.DeleteAsync(id)));
        }
        #endregion

        #region Performance
        [HttpGet("list-performance")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PaginatedList<AcademicPerformanceDto>>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAllPerformance([FromQuery] int pageIndex, [FromQuery] int pageSize, [FromQuery] AcademicPerformanceInfoDto filter = null)
        {
            return Response(new ApiResponse<PaginatedList<AcademicPerformanceDto>>(await _performance.GetAllAsync(pageIndex, pageSize, filter)));
        }

        [HttpGet("performance/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<AcademicPerformanceDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPerformanceById(Guid id)
        {
            return Response(new ApiResponse<AcademicPerformanceDto>(await _performance.GetByIdAsync(id)));
        }

        [HttpPost("performance")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddPerformance(AcademicPerformanceDto data)
        {
            return Response(new ApiResponse(await _performance.SaveAsync(Guid.Empty, data)));
        }

        [HttpPut("performance/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdatePerformance(Guid id, AcademicPerformanceDto data)
        {
            return Response(new ApiResponse(await _performance.SaveAsync(id, data)));
        }

        [HttpDelete("performance/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePerformance(Guid id)
        {
            return Response(new ApiResponse(await _performance.DeleteAsync(id)));
        }
        #endregion 

        #region Objective
        [HttpGet("list-objective")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PaginatedList<AcademicObjectiveDto>>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAllObjective([FromQuery] int pageIndex, [FromQuery] int pageSize, [FromQuery] AcademicObjectiveInfoDto filter = null)
        {
            return Response(new ApiResponse<PaginatedList<AcademicObjectiveDto>>(await _objective.GetAllAsync(pageIndex, pageSize, filter)));
        }

        [HttpGet("objective/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<AcademicObjectiveDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetObjectiveById(Guid id)
        {
            return Response(new ApiResponse<AcademicObjectiveDto>(await _objective.GetByIdAsync(id)));
        }

        [HttpPost("objective")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddObjective(AcademicObjectiveDto data)
        {
            return Response(new ApiResponse(await _objective.SaveAsync(Guid.Empty, data)));
        }

        [HttpPut("objective/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateObjective(Guid id, AcademicObjectiveDto data)
        {
            return Response(new ApiResponse(await _objective.SaveAsync(id, data)));
        }

        [HttpDelete("objective/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteObjective(Guid id)
        {
            return Response(new ApiResponse(await _objective.DeleteAsync(id)));
        }
        #endregion 

        #region Essential Knowledge
        [HttpGet("list-essential-knowledge")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PaginatedList<AcademicEssentialKnowledgeDto>>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAllEssentialKnowledge([FromQuery] int pageIndex, [FromQuery] int pageSize, [FromQuery] AcademicEssentialKnowledgeInfoDto filter = null)
        {
            return Response(new ApiResponse<PaginatedList<AcademicEssentialKnowledgeDto>>(await _knowledge.GetAllAsync(pageIndex, pageSize, filter)));
        }

        [HttpGet("essential-knowledge/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<AcademicEssentialKnowledgeDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetEssentialKnowledgeById(Guid id)
        {
            return Response(new ApiResponse<AcademicEssentialKnowledgeDto>(await _knowledge.GetByIdAsync(id)));
        }

        [HttpPost("essential-knowledge")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddEssentialKnowledge(AcademicEssentialKnowledgeDto data)
        {
            return Response(new ApiResponse(await _knowledge.SaveAsync(Guid.Empty, data)));
        }

        [HttpPut("essential-knowledge/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateEssentialKnowledge(Guid id, AcademicEssentialKnowledgeDto data)
        {
            return Response(new ApiResponse(await _knowledge.SaveAsync(id, data)));
        }

        [HttpDelete("essential-knowledge/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteEssentialKnowledge(Guid id)
        {
            return Response(new ApiResponse(await _knowledge.DeleteAsync(id)));
        }
        #endregion 

        #region Metodology
        [HttpGet("list-metodology")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PaginatedList<AcademicMethodologyDto>>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAllMetodology([FromQuery] int pageIndex, [FromQuery] int pageSize, [FromQuery] AcademicMethodologyInfoDto filter = null)
        {
            return Response(new ApiResponse<PaginatedList<AcademicMethodologyDto>>(await _metodology.GetAllAsync(pageIndex, pageSize, filter)));
        }

        [HttpGet("metodology/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<AcademicMethodologyDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMetodologyById(Guid id)
        {
            return Response(new ApiResponse<AcademicMethodologyDto>(await _metodology.GetByIdAsync(id)));
        }

        [HttpPost("metodology")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddMetodology(AcademicMethodologyDto data)
        {
            return Response(new ApiResponse(await _metodology.SaveAsync(Guid.Empty, data)));
        }

        [HttpPut("metodology/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateMetodology(Guid id, AcademicMethodologyDto data)
        {
            return Response(new ApiResponse(await _metodology.SaveAsync(id, data)));
        }

        [HttpDelete("metodology/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteMetodology(Guid id)
        {
            return Response(new ApiResponse(await _metodology.DeleteAsync(id)));
        }
        #endregion 
    }
}