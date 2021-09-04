using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Meadow.Foundation.Web.Maple.Server
{
    public interface IActionResult
    {
        void ExecuteResult(HttpListenerContext context);
        Task ExecuteResultAsync(HttpListenerContext context);
    }

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
            context.Response.ContentLength64 = data.LongLength;

            await context.Response.OutputStream.WriteAsync(data, 0, data.Length);
            context.Response.Close();
        }
    }

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

    public class OkResult : StatusCodeResult
    {
        public OkResult()
            : base(HttpStatusCode.OK)
        {

        }
    }

    public interface IOutputFormatter
    {
        public byte[] FormatContent(object content);
    }

    public class TextOutputFormatter : IOutputFormatter
    {
        public byte[] FormatContent(object content)
        {
            return Encoding.UTF8.GetBytes(content.ToString());
        }
    }

    public class JsonOutputFormatter : IOutputFormatter
    {
        public byte[] FormatContent(object content)
        {
            var json = SimpleJsonSerializer.JsonSerializer.SerializeObject(content);
            return Encoding.UTF8.GetBytes(json);
        }
    }

    public abstract class ObjectResult : ActionResult
    {
        public object Value { get; }
        public int? StatusCode { get; set; }

        public IOutputFormatter Formatter { get; set; }

        public ObjectResult(object value)
        {
            Value = value;
        }

        public override void ExecuteResult(HttpListenerContext context)
        {
            ExecuteResultAsync(context).Wait();
        }

        public override async Task ExecuteResultAsync(HttpListenerContext context)
        {
            context.Response.StatusCode = StatusCode ?? 200;

            // default to text output if nothing was set
            var formatter = Formatter ?? new TextOutputFormatter();

            var binaryContent = formatter.FormatContent(Value);

            await WriteOutputStream(context, binaryContent);
        }
    }

    public class OkObjectResult : ObjectResult
    {
        public OkObjectResult(object value)
            : base(value)
        {

        }
    }

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