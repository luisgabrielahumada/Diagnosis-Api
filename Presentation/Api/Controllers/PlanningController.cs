using Application.Dto;
using Application.Dto.Common;
using Infrastructure.Dto;
using Infrastructure.Dto.Filters;
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
    public class PlanningController : BaseApiController
    {
        private readonly IPlanningService _planning;
        private readonly IPlanningCompetencieService _planningCompetencie;
        private readonly IPlanningPerformanceService _planningPerformance;
        private readonly IPlanningUnitsService _planningUnit;
        private readonly IPlanningMethodologyService _metodology;
        private readonly IPlanningCycleService _planningCycle;
        private readonly ICycleObjectiveService _cycleObjective;
        private readonly ICycleReviewsService _cycleReviews;
        private readonly ICycleKnowledgeService _cycleKnowledge;

        public PlanningController(IPlanningService planning, IPlanningCompetencieService planningCompetencie,
                                  IPlanningPerformanceService planningPerformance, IPlanningUnitsService planningUnit,
                                  IPlanningMethodologyService metodology, IPlanningCycleService planningCycle,
                                  ICycleObjectiveService cycleObjective, ICycleReviewsService cycleReviews,
                                  ICycleKnowledgeService cycleKnowledge)
        {
            _planning = planning;
            _planningCompetencie = planningCompetencie;
            _planningPerformance = planningPerformance;
            _planningCycle = planningCycle;
            _metodology = metodology;
            _planningUnit = planningUnit;
            _cycleObjective = cycleObjective;
            _cycleReviews = cycleReviews;
            _cycleKnowledge = cycleKnowledge;
        }

        #region Planning
        [HttpGet("list-planning")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PlanningInboxDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAllPlanning([FromQuery] int pageIndex, [FromQuery] int pageSize, [FromQuery] PlanningInfoDto filter)
        {
            return Response(new ApiResponse<PlanningInboxDto>(await _planning.GetAllAsync(pageIndex, pageSize, filter)));
        }

        [HttpGet("planning/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PlanningDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPlanningById(Guid id)
        {
            return Response(new ApiResponse<PlanningDto>(await _planning.GetByIdAsync(id)));
        }

        [HttpPost("create-planning")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<Guid>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddPlanning(PlanningDto data)
        {
            return Response(new ApiResponse<Guid>(await _planning.CreateAsync(data)));
        }

        [HttpPost("{id}/clone-planning")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ClonePlanning(Guid id, PlanningCloneLogDto data)
        {
            return Response(new ApiResponse(await _planning.CloneAsync(id, data)));
        }

        [HttpPut("edit/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdatePlanning(Guid id, PlanningDto data)
        {
            return Response(new ApiResponse(await _planning.UpdateAsync(id, data)));
        }

        [HttpDelete("remove/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePlanning(Guid id)
        {
            return Response(new ApiResponse(await _planning.DeleteAsync(id)));
        }
        #endregion

        #region Planning Competencies
        [HttpGet("list-planning-competencies/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<PlanningItemDto>>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAllPlanningCompetency(Guid id)
        {
            return Response(new ApiResponse<List<PlanningItemDto>>(await _planningCompetencie.GetAllAsync(id)));
        }

        [HttpPost("selected/{id}/planning-competence")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddPlanningCompetency(Guid id, PlanningItemDto data)
        {
            return Response(new ApiResponse(await _planningCompetencie.SelectAsync(id, data)));
        }

        [HttpPut("unselected/{id}/planning-competence")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePlanningCompetency(Guid id)
        {
            return Response(new ApiResponse(await _planningCompetencie.UnSelectAsync(id)));
        }
        #endregion

        #region Planning Performance
        [HttpGet("list-planning-performances/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<PlanningItemDto>>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAllPlanningPerformance(Guid id)
        {
            return Response(new ApiResponse<List<PlanningItemDto>>(await _planningPerformance.GetAllAsync(id)));
        }

        [HttpPost("selected/{id}/planning-performance")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddPlanningPerformance(Guid id, PlanningItemDto data)
        {
            return Response(new ApiResponse(await _planningPerformance.SelectAsync(id, data)));
        }

        [HttpPut("unselected/{id}/planning-performance")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePlanningPerformance(Guid id)
        {
            return Response(new ApiResponse(await _planningPerformance.UnSelectAsync(id)));
        }

        
        #endregion 

        #region Planning Methodoly
        [HttpGet("list-planning-methology/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<PlanningItemDto>>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAllPlanningMethology(Guid id)
        {
            return Response(new ApiResponse<List<PlanningItemDto>>(await _metodology.GetAllAsync(id)));
        }

        [HttpPost("selected/{id}/planning-methology")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddPlanningMethology(Guid id, PlanningItemDto data)
        {
            return Response(new ApiResponse(await _metodology.SelectAsync(id, data)));
        }

        [HttpPut("unselected/{id}/planning-methology")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePlanningMethology(Guid id)
        {
            return Response(new ApiResponse(await _metodology.UnSelectAsync(id)));
        }
        #endregion 

        #region Planning Units
        [HttpGet("list-planning-units/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<PlanningItemDto>>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAllPlanningUnits(Guid id)
        {
            return Response(new ApiResponse<List<PlanningItemDto>>(await _planningUnit.GetAllAsync(id)));
        }

        [HttpPost("selected/{id}/planning-unit")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddPlanningUnit(Guid id, PlanningItemDto data)
        {
            return Response(new ApiResponse(await _planningUnit.SelectAsync(id, data)));
        }

        [HttpPut("unselected/{id}/planning-unit")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePlanningUnit(Guid id)
        {
            return Response(new ApiResponse(await _planningUnit.UnSelectAsync(id)));
        }
        #endregion

        #region Planning Cycle
        [HttpGet("list-planning-cycles/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PaginatedList<PlanningCycleDto>>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAllPlanningCycle(Guid id, int pageIndex, int pageSize, [FromQuery] PlanningCycleInfoDto filters)
        {
            return Response(new ApiResponse<PaginatedList<PlanningCycleDto>>(await _planningCycle.GetAllAsync(id, pageIndex, pageSize, filters)));
        }

        [HttpGet("planning-cycle/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PlanningCycleDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPlanningCycleById(Guid id)
        {
            return Response(new ApiResponse<PlanningCycleDto>(await _planningCycle.GetByIdAsync(id)));
        }

        [HttpPost("planning-cycle")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddPlanningCycle(PlanningCycleDto data)
        {
            return Response(new ApiResponse(await _planningCycle.SaveAsync(Guid.Empty, data)));
        }

        [HttpPut("planning-cycle/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdatePlanningCycle(Guid id, PlanningCycleDto data)
        {
            return Response(new ApiResponse(await _planningCycle.SaveAsync(id, data)));
        }

        [HttpDelete("planning-cycle/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeletePlanningCycle(Guid id)
        {
            return Response(new ApiResponse(await _planningCycle.DeleteAsync(id)));
        }

        [HttpGet("planning-units-by-objective/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<LookupDto>>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPlanningUnitById(Guid id)
        {
            return Response(new ApiResponse<List<LookupDto>>(await _planningCycle.GetPlanningUnitsByPlanningCycle(id)));
        }

        [HttpGet("list-planning-performances/{id}/planningCycle")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<PlanningItemDto>>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAllPlanningPerformanceCycle(Guid id,Guid? cycleId)
        {
            return Response(new ApiResponse<List<PlanningItemDto>>(await _planningCycle.GetAllPlanningPerformancesByPlanningCycle(id, cycleId)));
        }

        [HttpPost("selected/{id}/cycle-performance")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddCyclePerformance(Guid id, PlanningItemDto data)
        {
            return Response(new ApiResponse(await _planningCycle.SelectAsync(id, data)));
        }

        [HttpPut("unselected/{id}/cycle-performance")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCyclePerformance(Guid id)
        {
            return Response(new ApiResponse(await _planningCycle.UnSelectAsync(id)));
        }
        #endregion

        #region Essential Knowledge
        [HttpGet("essential-knowledge/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PlanningUnitDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetPlanningUnitsById(Guid id)
        {
            return Response(new ApiResponse<PlanningUnitDto>(await _planningUnit.GetByIdAsync(id)));
        }

        //[HttpPost("essential-knowledge")]
        //[ProducesResponseType(StatusCodes.Status201Created)]
        //[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        //public async Task<IActionResult> AddPlanningUnits(PlanningUnitsDto data)
        //{
        //    return Response(new ApiResponse(await _planningUnit.SaveAsync(data)));
        //}

        //[HttpPut("essential-knowledge/{id}")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        //public async Task<IActionResult> UpdatePlanningUnits(Guid id, PlanningUnitsDto data)
        //{
        //    return Response(new ApiResponse(await _planningUnit.SaveAsync(id, data)));
        //}

        //[HttpDelete("essential-knowledge/{id}")]
        //[ProducesResponseType(StatusCodes.Status204NoContent)]
        //[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        //public async Task<IActionResult> DeletePlanningUnits(Guid id)
        //{
        //    return Response(new ApiResponse(await _planningUnit.DeleteAsync(id)));
        //}
        #endregion 

        #region Cycle Objective
        [HttpGet("list-cycle-objectives/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<PlanningItemDto>>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAllCycleObject(Guid id)
        {
            return Response(new ApiResponse<List<PlanningItemDto>>(await _cycleObjective.GetAllAsync(id)));
        }

        [HttpGet("cycle-objective/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<CycleObjectiveDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCycleObjectById(Guid id)
        {
            return Response(new ApiResponse<CycleObjectiveDto>(await _cycleObjective.GetByIdAsync(id)));
        }

        [HttpPost("selected/{id}/cycle-objective")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddCycleObjective(Guid id, PlanningItemDto data)
        {
            return Response(new ApiResponse(await _cycleObjective.SelectAsync(id, data)));
        }

        [HttpPut("unselected/{id}/cycle-objective")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCycleObjective(Guid id)
        {
            return Response(new ApiResponse(await _cycleObjective.UnSelectAsync(id)));
        }

      
        #endregion 

        #region Cycle Knowledge
        [HttpGet("list-cycle-knowledges/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PaginatedList<CycleKnowledgeDto>>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAllCycleKnowledge(Guid id, [FromQuery] Guid obj)
        {
            return Response(new ApiResponse<List<PlanningItemDto>>(await _cycleKnowledge.GetAllAsync(id, obj)));
        }

        [HttpGet("cycle-knowledge/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<CycleKnowledgeDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCycleKnowledgeById(Guid id)
        {
            return Response(new ApiResponse<CycleKnowledgeDto>(await _cycleKnowledge.GetByIdAsync(id)));
        }

        [HttpGet("cycle-objective-knowledge/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<LookupDto>>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCycleObjetiveKnowledgeById(Guid id)
        {
            return Response(new ApiResponse<List<LookupDto>>(await _cycleKnowledge.GetCycleObjectiveKnowledgeById(id)));
        }

        [HttpPost("selected/{id}/cycle-knowledge")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddCycleKnowledge(Guid id, PlanningItemDto data)
        {
            return Response(new ApiResponse(await _cycleKnowledge.SelectAsync(id, data)));
        }

        [HttpPut("unselected/{id}/cycle-knowledge")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCycleKnowledge(Guid id)
        {
            return Response(new ApiResponse(await _cycleKnowledge.UnSelectAsync(id)));
        }
        #endregion 

        #region Cycle Reviews
        [HttpGet("list-cycle-reviews/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PaginatedList<CycleReviewDto>>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAllCycleReviews(Guid id, int pageIndex, int pageSize)
        {
            return Response(new ApiResponse<PaginatedList<CycleReviewDto>>(await _cycleReviews.GetAllAsync(id, pageIndex, pageSize)));
        }

        [HttpGet("cycle-review/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<CycleReviewDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCycleReviewsById(Guid id)
        {
            return Response(new ApiResponse<CycleReviewDto>(await _cycleReviews.GetByIdAsync(id)));
        }

        [HttpPost("cycle-review")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> AddCycleReviews(CycleReviewDto data)
        {
            return Response(new ApiResponse(await _cycleReviews.SaveAsync(Guid.Empty, data)));
        }

        [HttpPut("cycle-review/{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateCycleReviews(Guid id, CycleReviewDto data)
        {
            return Response(new ApiResponse(await _cycleReviews.SaveAsync(id, data)));
        }

        [HttpDelete("cycle-review/{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCycleReviews(Guid id)
        {
            return Response(new ApiResponse(await _cycleReviews.DeleteAsync(id)));
        }
        #endregion 
    }
}