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
    public class LogController : BaseApiController
    {
        private readonly ILoggingService _process;

        public LogController(ILoggingService process)
        {
            _process = process;
        }

        //[AllowAnonymous]
        [HttpGet("getAll")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PaginatedList<LogDto>>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAll([FromQuery] int pageIndex, [FromQuery] int pageSize, [FromQuery] LogInfoDto req)
        {
            return Response(new ApiResponse<PaginatedList<LogDto>>(await _process.GetAllAsync(req, pageIndex, pageSize)));
        }
    }
}