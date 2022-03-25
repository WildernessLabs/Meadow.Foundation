using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Meadow.Foundation.Web.Maple
{
    internal class MapleSerializationStrategy : SimpleJson.PocoJsonSerializerStrategy
    {
        protected override object SerializeEnum(Enum p)
        {
            return p.ToString();
        }
    }

    public class JsonResult : ActionResult
    {
        public int? StatusCode { get; set; }
        public object Value { get; set; }
        
        public JsonResult(object value)
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
            context.Response.ContentType = ContentTypes.Application_Json;

            // TODO: creating the strategy on every call seems like bad form
            var json = SimpleJson.SimpleJson.SerializeObject(Value, new MapleSerializationStrategy());
            var binaryContent = Encoding.UTF8.GetBytes(json);
            await WriteOutputStream(context, binaryContent);
        }
    }
}