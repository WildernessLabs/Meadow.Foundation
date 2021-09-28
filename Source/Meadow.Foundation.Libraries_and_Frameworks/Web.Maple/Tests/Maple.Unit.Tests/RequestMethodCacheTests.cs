using Meadow.Foundation.Web.Maple.Server;
using Meadow.Foundation.Web.Maple.Server.Routing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
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

    [TestClass]
    public class RequestMethodCacheTests
    {
        [TestMethod]
        public void TestStringMidParameterPositive()
        {
            RequestMethodCache cache = new RequestMethodCache(null);

            cache.AddType(typeof(TestHandler));

            var p = "hello";

            var m = typeof(TestHandler).GetMethod("MethodWithParamMid");
            var info = cache.Match("GET", $"/foo/baz/{p}/bar", out object param);

            Assert.AreEqual(m, info.Method);
            Assert.AreEqual(p, param);
        }

        [TestMethod]
        public void TestStringMidParameterNegative1()
        {
            RequestMethodCache cache = new RequestMethodCache(null);

            cache.AddType(typeof(TestHandler));

            var p = "hello";
            var info = cache.Match("GET", $"/foo/baz/{p}/nope", out object param);

            Assert.IsNull(info);
        }

        [TestMethod]
        public void TestStringEndParameterPositive()
        {
            RequestMethodCache cache = new RequestMethodCache(null);

            cache.AddType(typeof(TestHandler));

            var p = "hello";

            var m = typeof(TestHandler).GetMethod("MethodWithStringParamEnd");
            var info = cache.Match("GET", $"/foo/bar/{p}", out object param);

            Assert.AreEqual(m, info.Method);
            Assert.AreEqual(p, param);
        }

        [TestMethod]
        public void TestStringEndParameterNegative()
        {
            RequestMethodCache cache = new RequestMethodCache(null);

            cache.AddType(typeof(TestHandler));

            var p = "hello";

            var m = typeof(TestHandler).GetMethod("MethodWithStringParamEnd");
            var info = cache.Match("GET", $"/foo/bar/baz/bad", out object param);

            Assert.IsNull(info);
        }

        [TestMethod]
        public void TestIntParameter()
        {
            RequestMethodCache cache = new RequestMethodCache(null);

            cache.AddType(typeof(TestHandler));

            var p = 1234;

            var m = typeof(TestHandler).GetMethod("MethodWithIntParamEnd");
            var info = cache.Match("GET", $"/foo/int/{p}", out object param);

            Assert.AreEqual(m, info.Method);
            Assert.AreEqual(p, param);
        }

        [TestMethod]
        public void TestGuidParameter()
        {
            RequestMethodCache cache = new RequestMethodCache(null);

            cache.AddType(typeof(TestHandler));

            var p = Guid.NewGuid();

            var m = typeof(TestHandler).GetMethod("MethodWithGuidParamEnd");
            var info = cache.Match("GET", $"/foo/gid/{p}", out object param);

            Assert.AreEqual(m, info.Method);
            Assert.AreEqual(p, param);
        }
    }
}
