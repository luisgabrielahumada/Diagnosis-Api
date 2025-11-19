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
    public class SettingController : BaseApiController
    {
        private readonly ISettingService _process;

        public SettingController(ISettingService process)
        {
            _process = process;
        }

        [HttpGet("getAll")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ConfigurationDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAll()
        {
            return Response(new ApiResponse<ConfigurationDto>(await _process.GetAllAsync()));
        }


        [HttpGet("{code}/code")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ParameterDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get(string code)
        {
            return Response(new ApiResponse<ParameterDto>(await _process.GetAsync(code)));
        }


        [HttpGet("reload")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> LoadParameters()
        {
            return Response(new ApiResponse(await _process.LoadParametersAsync()));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<ParameterDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            return Response(new ApiResponse<ParameterDto>(await _process.GetByIdAsync(id)));
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, ParameterDto data)
        {
            return Response(new ApiResponse(await _process.SaveAsync(id, data)));
        }

    }
}