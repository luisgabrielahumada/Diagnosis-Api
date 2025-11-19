using Application.Dto;
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
    public class AdminConfigurationcontroller : BaseApiController
    {
        private readonly IAdminConfigurationService _process;

        public AdminConfigurationcontroller(IAdminConfigurationService process)
        {
            _process = process;
        }

        //[AllowAnonymous]
        [HttpGet("getAll")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<List<AdminConfigurationDto>>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAll()
        {
            return Response(new ApiResponse<List<AdminConfigurationDto>>(await _process.GetAllAsync()));
        }


        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, AdminConfigurationDto data)
        {
            return Response(new ApiResponse(await _process.SaveAsync(id, data)));
        }


        [HttpGet("reloadParameters")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ReloadParameters()
        {
            return Response(new ApiResponse(await _process.ReloadParametersAsync()));
        }
    }
}