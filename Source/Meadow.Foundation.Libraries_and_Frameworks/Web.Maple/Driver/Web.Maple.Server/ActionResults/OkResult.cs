using System.Net;

namespace Meadow.Foundation.Web.Maple.Server
{
    public class OkResult : StatusCodeResult
    {
        public OkResult()
            : base(HttpStatusCode.OK)
        {

        }
    }
}