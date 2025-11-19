using Hangfire;
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
    public class UserController : BaseApiController
    {
        private readonly IUserService _process;

        public UserController(IUserService process)
        {
            _process = process;
        }

        //[AllowAnonymous]
        [HttpGet("currentUser-list")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PaginatedList<UserDto>>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetCurrentUserAll([FromQuery]int pageIndex, [FromQuery] int pageSize, [FromQuery] UserInfoDto filter)
        {
            return Response(new ApiResponse<PaginatedList<UserDto>>(await _process.GetCurrentUserAllAsync(pageIndex, pageSize, filter)));
        }

        [HttpGet("list")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<PaginatedList<UserDto>>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAll([FromQuery] int pageIndex, [FromQuery] int pageSize, [FromQuery] UserInfoDto filter)
        {
            return Response(new ApiResponse<PaginatedList<UserDto>>(await _process.GetAllAsync(pageIndex, pageSize, filter)));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse<UserDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(int id)
        {
            return Response(new ApiResponse<UserDto>(await _process.GetByIdAsync(id)));
        }

        [HttpPost("{id}/initial-configuration")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> InitialConfigurationRoleAsync(Guid id, CampusRoleDto data)
        {
            return Response(new ApiResponse(await _process.InitialConfigurationRoleAsync(id,data)));
        }


        [HttpPost()]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Add(UserDto data)
        {
            return Response(new ApiResponse(await _process.SaveAsync(Guid.Empty, data)));
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, UserDto data)
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