using System.Net;
using System.Threading.Tasks;

namespace Meadow.Foundation.Web.Maple.Server
{
    public static class RequestHandlerExtensions
    {
        public static void BadRequest(this RequestHandlerBase handler)
        {
            handler.Context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            handler.Send();
        }

        public static void NotFound(this RequestHandlerBase handler)
        {
            handler.Context.Response.StatusCode = (int)HttpStatusCode.NotFound;
            handler.Send();
        }

        public static void Forbidden(this RequestHandlerBase handler)
        {
            handler.Context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            handler.Send();
        }

        public static void ServerError(this RequestHandlerBase handler)
        {
            handler.Context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            handler.Send();
        }

        public static void Ok(this RequestHandlerBase handler)
        {
            handler.Context.Response.StatusCode = (int)HttpStatusCode.OK;
            handler.Send();
        }

        public static async Task Ok(this RequestHandlerBase handler, string content)
        {
            handler.Context.Response.StatusCode = (int)HttpStatusCode.OK;
            handler.Context.Response.ContentType = ContentTypes.Application_Text;
            await handler.Send(content);
        }

        public static async Task Json(this RequestHandlerBase handler, object content)
        {
            handler.Context.Response.StatusCode = (int)HttpStatusCode.OK;
            handler.Context.Response.ContentType = ContentTypes.Application_Json;
            await handler.Send(content);
        }
    }
}