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
    public class PeriodController : BaseApiController
    {
        private readonly IAcademicPeriodService _process;

        public PeriodController(IAcademicPeriodService process)
        {
            _process = process;
        }

        //[AllowAnonymous]
        [HttpGet("list")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PaginatedList<AcademicPeriodDto>>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAll([FromQuery] int pageIndex, [FromQuery] int pageSize, [FromQuery] PeriodInfoDto filter)
        {
            return Response(new ApiResponse<PaginatedList<AcademicPeriodDto>>(await _process.GetAllAsync(pageIndex, pageSize, filter)));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<AcademicPeriodDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            return Response(new ApiResponse<AcademicPeriodDto>(await _process.GetByIdAsync(id)));
        }

        [HttpPost()]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Add(AcademicPeriodDto data)
        {
            return Response(new ApiResponse(await _process.SaveAsync(Guid.Empty, data)));
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, AcademicPeriodDto data)
        {
            return Response(new ApiResponse(await _process.SaveAsync(id, data)));
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id)
        {
            return Response(new ApiResponse(await _process.DeleteAsync(id)));
        }
    }
}