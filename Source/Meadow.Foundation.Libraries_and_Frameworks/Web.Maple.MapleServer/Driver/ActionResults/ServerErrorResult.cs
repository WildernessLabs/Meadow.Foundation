using System;

namespace Meadow.Foundation.Web.Maple
{
    public class ServerErrorResult : ObjectResult
    {
        public ServerErrorResult()
            : base(null)
        {
        }

        public ServerErrorResult(Exception ex)
            : base(ex)
        {
            this.StatusCode = 500;
        }
    }
}