using System;
using System.Net;
using System.Threading.Tasks;

namespace Meadow.Foundation.Web.Maple.Server
{
    public static class RequestHandlerExtensions
    {
        public static IActionResult BadRequest(this RequestHandlerBase handler)
        {
            return new StatusCodeResult(HttpStatusCode.BadRequest);
        }

        public static IActionResult NotFound(this RequestHandlerBase handler)
        {
            return new NotFoundResult();
        }

        public static IActionResult Forbidden(this RequestHandlerBase handler)
        {
            return new StatusCodeResult(HttpStatusCode.Forbidden);
        }

        public static IActionResult ServerError(this RequestHandlerBase handler, Exception ex)
        {
            return new ServerErrorResult(ex);
        }

        public static IActionResult ServerError(this RequestHandlerBase handler)
        {
            return new ServerErrorResult();
        }

        public static IActionResult Ok(this RequestHandlerBase handler)
        {
            return new OkResult();
        }

        public static IActionResult Ok(this RequestHandlerBase handler, string content)
        {
            return new OkObjectResult(content);
        }

        public static IActionResult Json(this RequestHandlerBase handler, object content)
        {
            return new JsonResult(content);
        }
    }
}