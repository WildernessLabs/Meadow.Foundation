using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Meadow.Foundation.Web.Maple
{
    public class ContentResult : ActionResult
    {
        public string Content { get; set; }
        public string ContentType { get; set; }
        public int? StatusCode { get; set; }

        public ContentResult()
        {

        }

        public override void ExecuteResult(HttpListenerContext context)
        {
            ExecuteResultAsync(context).Wait();
        }

        public override async Task ExecuteResultAsync(HttpListenerContext context)
        {
            context.Response.StatusCode = StatusCode ?? 200;
            context.Response.ContentType = ContentType;
            var binaryContent = Encoding.UTF8.GetBytes(Content);
            await WriteOutputStream(context, binaryContent);
        }
    }
}