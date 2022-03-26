namespace Meadow.Foundation.Web.Maple
{
    public interface IOutputFormatter
    {
        public byte[] FormatContent(object content);
    }
}