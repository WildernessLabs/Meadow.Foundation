using System.Net;

namespace Meadow.Foundation.Web.Maple
{
    public class OkResult : StatusCodeResult
    {
        public OkResult()
            : base(HttpStatusCode.OK)
        {

        }
    }
}