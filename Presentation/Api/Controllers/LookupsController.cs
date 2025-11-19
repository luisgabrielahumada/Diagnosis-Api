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
    public class LookupsController : BaseApiController
    {
        private readonly ILookupService _process;

        public LookupsController(ILookupService process)
        {
            _process = process;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<LookupBundleDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAllLookupsAsync()
        {
            return Response(new ApiResponse<LookupBundleDto>(await _process.GetAllLookupsAsync()));
        }
    }
}