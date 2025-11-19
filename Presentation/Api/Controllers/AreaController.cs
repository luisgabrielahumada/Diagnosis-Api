using Application.Dto;
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
    public class AreaController : BaseApiController
    {
        private readonly IAcademicAreaService _process;

        public AreaController(IAcademicAreaService process)
        {
            _process = process;
        }

        //[AllowAnonymous]
        [HttpGet("list")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PaginatedList<AcademicAreaDto>>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAll([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = int.MaxValue, [FromQuery] AcademicAreaInfoDto filter = null)
        {
            return Response(new ApiResponse<PaginatedList<AcademicAreaDto>>(await _process.GetAllAsync(pageIndex, pageSize, filter)));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<AcademicAreaDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            return Response(new ApiResponse<AcademicAreaDto>(await _process.GetByIdAsync(id)));
        }

        [HttpPost()]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Add(CreateAcademicAreaDto data)
        {
            return Response(new ApiResponse(await _process.SaveAsync(Guid.Empty, data)));
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, CreateAcademicAreaDto data)
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

        //[HttpGet("list-area-by-campus")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(ApiResponse<PaginatedList<AcademicAreaCampusReviewerDto>>), StatusCodes.Status404NotFound)]
        //public async Task<IActionResult> GetAreaByCampusAll([FromQuery] int pageIndex, [FromQuery] int pageSize, [FromQuery] AreaCampusReviewerInfoDto filter)
        //{
        //    return Response(new ApiResponse<PaginatedList<AcademicAreaCampusReviewerDto>>(await _areaCampusReviewerService.GetAllAsync(pageIndex, pageSize, filter)));
        //}

        //[HttpPost("area-by-campus")]
        //[ProducesResponseType(StatusCodes.Status201Created)]
        //[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        //public async Task<IActionResult> AddAcademicAreaCampusReviewe(AcademicAreaCampusReviewerDto data)
        //{
        //    return Response(new ApiResponse(await _areaCampusReviewerService.SaveAsync(data)));
        //}

        //[HttpPut("area-by-campus/{id}")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        //public async Task<IActionResult> UpdateAcademicAreaCampusReviewe(AcademicAreaCampusReviewerDto data)
        //{
        //    return Response(new ApiResponse(await _areaCampusReviewerService.SaveAsync(data)));
        //}

        //[HttpDelete("{area}/area-by-campus/{campus}")]
        //[ProducesResponseType(StatusCodes.Status204NoContent)]
        //[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        //public async Task<IActionResult> Delete(Guid area,Guid campus)
        //{
        //    return Response(new ApiResponse(await _areaCampusReviewerService.DeleteAsync(area, campus)));
        //}
    }
}