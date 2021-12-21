using System.Net;
using System.Threading.Tasks;

namespace Meadow.Foundation.Web.Maple.Server
{
    public class StatusCodeResult : ActionResult
    {
        public int StatusCode { get; }

        public StatusCodeResult(HttpStatusCode statusCode)
            : this((int)statusCode)
        {
        }

        public StatusCodeResult(int statusCode)
        {
            StatusCode = statusCode;
        }

        public override void ExecuteResult(HttpListenerContext context)
        {
            ExecuteResultAsync(context).Wait();
        }

        public override async Task ExecuteResultAsync(HttpListenerContext context)
        {
            context.Response.StatusCode = StatusCode;
            context.Response.ContentType = ContentTypes.Default;
            await WriteOutputStream(context, null);
        }
    }
}