using System;

namespace Meadow.Foundation.Web.Maple.Server
{
    public class ServerErrorResult : ObjectResult
    {
        public ServerErrorResult(Exception ex)
            : base(ex)
        {
            this.StatusCode = 500;
        }
    }
}