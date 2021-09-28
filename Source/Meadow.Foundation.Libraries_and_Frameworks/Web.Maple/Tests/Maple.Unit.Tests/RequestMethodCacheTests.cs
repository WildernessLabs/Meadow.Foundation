using Meadow.Foundation.Web.Maple.Server;
using Meadow.Foundation.Web.Maple.Server.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics;
using System.Net;

namespace Maple.Unit.Tests
{
    internal class TestHandler : IRequestHandler
    {
        public HttpListenerContext Context { get; set; }

        public bool IsReusable => true;

        public void Dispose()
        {
        }

        [HttpGet("/foo/bar/{paramName}")]
        public void MethodWithParam(string paramName)
        {
            Debug.WriteLine($"{paramName}");
        }
    }

    [TestClass]
    public class RequestMethodCacheTests
    {
        [TestMethod]
        public void TestMethod1()
        {
            RequestMethodCache cache = new RequestMethodCache(null);

            cache.AddType(typeof(TestHandler));
            
            var m = typeof(TestHandler).GetMethod("MethodWrithParam");

            cache.Add("GET", "/foo/bar/{paramName}", m);
        }
    }
}
