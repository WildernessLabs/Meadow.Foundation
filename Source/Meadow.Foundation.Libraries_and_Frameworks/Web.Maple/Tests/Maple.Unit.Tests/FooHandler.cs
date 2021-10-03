using Meadow.Foundation.Web.Maple.Server;
using Meadow.Foundation.Web.Maple.Server.Routing;
using System;
using System.Diagnostics;
using System.Net;

namespace Maple.Unit.Tests
{
    internal class FooHandler : IRequestHandler
    {
        public HttpListenerContext Context { get; set; }

        public bool IsReusable => true;

        public void Dispose()
        {
        }

        [HttpGet("gid/{paramName}")]
        public void MethodWithGuidParamEnd(Guid paramName)
        {
            Debug.WriteLine($"{paramName}");
        }

        [HttpGet("int/{paramName}")]
        public void MethodWithIntParamEnd(int paramName)
        {
            Debug.WriteLine($"{paramName}");
        }

        [HttpGet("bar/{paramName}")]
        public void MethodWithStringParamEnd(string paramName)
        {
            Debug.WriteLine($"{paramName}");
        }

        [HttpGet("baz/{paramName}/bar")]
        public void MethodWithParamMid(string paramName)
        {
            Debug.WriteLine($"{paramName}");
        }
    }
}
