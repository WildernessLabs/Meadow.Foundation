using System.Net;

namespace Meadow.Foundation.Web.Maple
{
    public class NotFoundResult : StatusCodeResult
    {
        public NotFoundResult()
            : base(HttpStatusCode.NotFound)
        {

        }
    }
}