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
    public class AuditLogController : BaseApiController
    {
        private readonly IAuditLogService _process;

        public AuditLogController(IAuditLogService process)
        {
            _process = process;
        }

        //[AllowAnonymous]
        [HttpGet("{table}/{recordId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PaginatedList<AuditLogDto>>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAll(Guid recordId, int pageIndex, int pageSize, string table = null)
        {
            return Response(new ApiResponse<PaginatedList<AuditLogDto>>(await _process.GetAllAsync(recordId,pageIndex, pageSize, table)));
        }
    }
}