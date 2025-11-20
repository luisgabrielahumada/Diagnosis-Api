using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Shared.Response;

namespace Web.Api.Code
{
    public class BaseApiController : Controller
    {
        protected new IActionResult Response(ApiResponse ar, bool onError = false, bool returnAlways200 = false, int statusCode = StatusCodes.Status403Forbidden)
        {
            if (!ar.Status)
            {
                if (onError)
                    return NotFound();

                return BadRequest(ar);
            }

            if (onError)
                return StatusCode(statusCode, ar);

            var content = ar;
            if (content == null)
                return returnAlways200 ? Ok() : ar.AsyncOperation ? (IActionResult)Accepted() : NoContent();

            return Ok(content);
        }
        protected new IActionResult Response<T>(ApiResponse<T> ar, bool onError = false, bool returnAlways200 = false, int statusCode = StatusCodes.Status403Forbidden)
        {
            return Response(ar.ToGeneric(), onError, returnAlways200, statusCode);
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
        }
    }
}