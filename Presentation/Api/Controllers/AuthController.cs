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
    public class AuthController : BaseApiController
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }


        //[AllowAnonymous]
        //[HttpPost("register")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        //public async Task<IActionResult> Register([FromBody] UsersDto model)
        //{
        //    var result = await _authService.RegisterUserAsync(model);
        //    return Response(new ApiResponse<bool>(result));
        //}


        [AllowAnonymous]
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Login([FromBody] LoginDto req)
        {
            var result = await _authService.LoginUserAsync(req);
            return Response(new ApiResponse<LoginResponseDto>(result));
        }

        [AllowAnonymous]
        [HttpPost("microsoft-login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> AutoLogin([FromBody] MicrosoftLoginDto req)
        {
            var result = await _authService.LoginMicrosoftAsync(req);
            return Response(new ApiResponse<LoginResponseDto>(result));
        }


        [AllowAnonymous]
        [HttpPost("renew-token")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ReNewToken([FromBody] SessionDto token)
        {
            var result = await _authService.ReNewToken(token);
            return Response(new ApiResponse(result));
        }
    }
}