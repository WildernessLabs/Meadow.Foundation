using System.Net;
using System.Threading.Tasks;

namespace Meadow.Foundation.Web.Maple.Server
{
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
}