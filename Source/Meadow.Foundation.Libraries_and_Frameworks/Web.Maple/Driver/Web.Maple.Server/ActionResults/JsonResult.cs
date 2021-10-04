using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Meadow.Foundation.Web.Maple.Server
{
    public class JsonResult : ActionResult
    {
        public int? StatusCode { get; set; }
        public object Value { get; set; }
        public SimpleJsonSerializer.DateTimeFormat DateTimeFormat { get; }

        public JsonResult(object value, SimpleJsonSerializer.DateTimeFormat dateTimeFormat = SimpleJsonSerializer.DateTimeFormat.Default)
        {
            Value = value;
            DateTimeFormat = dateTimeFormat;
        }

        public override void ExecuteResult(HttpListenerContext context)
        {
            ExecuteResultAsync(context).Wait();
        }

        public override async Task ExecuteResultAsync(HttpListenerContext context)
        {
            context.Response.StatusCode = StatusCode ?? 200;
            context.Response.ContentType = ContentTypes.Application_Json;

            var json = SimpleJsonSerializer.JsonSerializer.SerializeObject(Value, DateTimeFormat);
            var binaryContent = Encoding.UTF8.GetBytes(json);
            await WriteOutputStream(context, binaryContent);
        }
    }
}