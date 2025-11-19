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
    public class CampusesController : BaseApiController
    {
        private readonly ICampusService _process;

        public CampusesController(ICampusService process)
        {
            _process = process;
        }

        //[AllowAnonymous]
        [HttpGet("list")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PaginatedList<CampusDto>>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAll([FromQuery] int pageIndex = 1, [FromQuery] int pageSize = int.MaxValue, [FromQuery] CampusInfoDto filter = null)
        {
            return Response(new ApiResponse<PaginatedList<CampusDto>>(await _process.GetAllAsync(pageIndex, pageSize, filter)));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<CampusDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            return Response(new ApiResponse<CampusDto>(await _process.GetByIdAsync(id)));
        }

        [HttpGet("{id}/Configuration")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<CampusDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetConfigurationAsync(Guid id)
        {
            return Response(new ApiResponse<CampusDto>(await _process.GetConfigurationAsync(id)));
        }

        [HttpPost()]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Add(CampusDto data)
        {
            return Response(new ApiResponse(await _process.SaveAsync(Guid.Empty, data)));
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, CampusDto data)
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

        [HttpGet("{id}/teachers")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<LookupDto>>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTeachers(Guid id, [FromQuery]  Guid area,[FromQuery] Guid subject)
        {
            return Response(new ApiResponse<List<LookupDto>>(await _process.GetTeachersAsync(id, subject, area)));
        }

        //[HttpGet("all-with-areas")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(ApiResponse<List<CampusWithAreasDto>>), StatusCodes.Status404NotFound)]
        //public async Task<IActionResult> GetAllWithAreasAsync()
        //{
        //    return Response(new ApiResponse<List<CampusWithAreasDto>>(await _process.GetAllWithAreasAsync(1, int.MaxValue)));
        //}
    }
}