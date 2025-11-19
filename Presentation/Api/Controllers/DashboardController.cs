using Infrastructure.Dto;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Response;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Area(nameof(Controllers))]
    //[Authorize]
    [AllowAnonymous]
    public class DashboardController : BaseApiController
    {
        private readonly IDashboardService _process;

        public DashboardController(IDashboardService process)
        {
            _process = process;
        }

        //[AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<DashboardStatsDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDashboardStats()
        {
            return Response(new ApiResponse<DashboardStatsDto>(await _process.GetDashboardStatsAsync()));
        }
    }
}