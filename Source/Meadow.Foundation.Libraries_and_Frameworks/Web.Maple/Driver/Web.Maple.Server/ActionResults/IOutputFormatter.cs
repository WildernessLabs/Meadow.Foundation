namespace Meadow.Foundation.Web.Maple.Server
{
    public interface IOutputFormatter
    {
        public byte[] FormatContent(object content);
    }
}