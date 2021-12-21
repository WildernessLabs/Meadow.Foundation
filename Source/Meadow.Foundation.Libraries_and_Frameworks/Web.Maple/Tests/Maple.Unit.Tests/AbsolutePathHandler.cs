using Meadow.Foundation.Web.Maple.Server;
using Meadow.Foundation.Web.Maple.Server.Routing;
using System;
using System.Diagnostics;
using System.Net;

namespace Maple.Unit.Tests
{
    internal class AbsolutePathHandler : IRequestHandler
    {
        public HttpListenerContext Context { get; set; }

        public bool IsReusable => true;

        public void Dispose()
        {
        }

        [HttpGet("/foo/gid/{paramName}")]
        public void MethodWithGuidParamEnd(Guid paramName)
        {
            Debug.WriteLine($"{paramName}");
        }

        [HttpGet("/foo/int/{paramName}")]
        public void MethodWithIntParamEnd(int paramName)
        {
            Debug.WriteLine($"{paramName}");
        }

        [HttpGet("/foo/bar/{paramName}")]
        public void MethodWithStringParamEnd(string paramName)
        {
            Debug.WriteLine($"{paramName}");
        }

        [HttpGet("/foo/baz/{paramName}/bar")]
        public void MethodWithParamMid(string paramName)
        {
            Debug.WriteLine($"{paramName}");
        }
    }
}
