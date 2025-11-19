using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Response;

namespace Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Area(nameof(Controllers))]
    public class WebHookNotificationController : BaseApiController
    {
        private readonly IWebHookNotificacionService _process;

        public WebHookNotificationController(IWebHookNotificacionService process)
        {
            _process = process;
        }


        [AllowAnonymous]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetPayload()
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();
            var result = await _process.GePayloadAsync(body);
            return Response(new ApiResponse(result));
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetStatusPayload()
        {
            return Response(new ApiResponse(new ServiceResponse()));
        }
    }
}