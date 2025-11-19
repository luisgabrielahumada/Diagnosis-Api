using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Shared.Response;

namespace Web.Api.Code
{
    public class BaseApiController : Controller
    {
        protected new IActionResult Response(ApiResponse ar, bool notFoundOnError = false, bool returnAlways200 = false)
        {
            if (!ar.Status)
            {
                if (notFoundOnError)
                    return NotFound();

                return BadRequest(ar);
            }

            var content = ar;
            if (content == null)
                return returnAlways200 ? Ok() : ar.AsyncOperation ? (IActionResult)Accepted() : NoContent();

            return Ok(content);
        }

        protected new IActionResult Response<T>(ApiResponse<T> ar, bool notFoundOnError = false)
        {
            return Response(ar.ToGeneric(), notFoundOnError);
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);
        }
    }
}