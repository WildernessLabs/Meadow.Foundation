using System.Text;

namespace Meadow.Foundation.Web.Maple.Server
{
    public class JsonOutputFormatter : IOutputFormatter
    {
        public byte[] FormatContent(object content)
        {
            var json = SimpleJsonSerializer.JsonSerializer.SerializeObject(content);
            return Encoding.UTF8.GetBytes(json);
        }
    }
}