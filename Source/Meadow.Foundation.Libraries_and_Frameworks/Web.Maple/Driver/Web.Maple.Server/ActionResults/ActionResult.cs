using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Meadow.Foundation.Web.Maple.Server
{
    public abstract class ActionResult : IActionResult
    {
        protected ActionResult()
        {

        }

        public abstract void ExecuteResult(HttpListenerContext context);
        public abstract Task ExecuteResultAsync(HttpListenerContext context);

        protected async Task WriteOutputStream(HttpListenerContext context, byte[] data)
        {
            context.Response.ContentEncoding = Encoding.UTF8;
            if (data == null)
            {
                context.Response.ContentLength64 = 0;
            }
            else
            {
                context.Response.ContentLength64 = data.LongLength;
                await context.Response.OutputStream.WriteAsync(data, 0, data.Length);
            }
            context.Response.Close();
        }
    }
}