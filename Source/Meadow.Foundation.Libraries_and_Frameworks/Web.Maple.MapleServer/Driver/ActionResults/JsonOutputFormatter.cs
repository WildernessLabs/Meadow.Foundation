using System.Text;

namespace Meadow.Foundation.Web.Maple
{
    public class JsonOutputFormatter : IOutputFormatter
    {
        public byte[] FormatContent(object content)
        {
            // TODO: creating the strategy on every call seems like bad form
            var json = SimpleJson.SimpleJson.SerializeObject(content, new MapleSerializationStrategy());
            return Encoding.UTF8.GetBytes(json);
        }
    }
}