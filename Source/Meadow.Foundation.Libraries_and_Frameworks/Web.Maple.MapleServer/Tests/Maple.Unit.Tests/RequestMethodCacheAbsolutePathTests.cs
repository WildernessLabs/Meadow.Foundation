using Meadow.Foundation.Web.Maple;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Maple.Unit.Tests
{
    [TestClass]
    public class RequestMethodCacheAbsolutePathTests
    {
        [TestMethod]
        public void TestStringMidParameterPositive()
        {
            RequestMethodCache cache = new RequestMethodCache(null);

            cache.AddType(typeof(AbsolutePathHandler));

            var p = "hello";

            var m = typeof(AbsolutePathHandler).GetMethod("MethodWithParamMid");
            var info = cache.Match("GET", $"/foo/baz/{p}/bar", out object param);

            Assert.AreEqual(m, info.Method);
            Assert.AreEqual(p, param);
        }

        [TestMethod]
        public void TestStringMidParameterNegative1()
        {
            RequestMethodCache cache = new RequestMethodCache(null);

            cache.AddType(typeof(AbsolutePathHandler));

            var p = "hello";
            var info = cache.Match("GET", $"/foo/baz/{p}/nope", out object param);

            Assert.IsNull(info);
        }

        [TestMethod]
        public void TestStringEndParameterPositive()
        {
            RequestMethodCache cache = new RequestMethodCache(null);

            cache.AddType(typeof(AbsolutePathHandler));

            var p = "hello";

            var m = typeof(AbsolutePathHandler).GetMethod("MethodWithStringParamEnd");
            var info = cache.Match("GET", $"/foo/bar/{p}", out object param);

            Assert.AreEqual(m, info.Method);
            Assert.AreEqual(p, param);
        }

        [TestMethod]
        public void TestStringEndParameterNegative()
        {
            RequestMethodCache cache = new RequestMethodCache(null);

            cache.AddType(typeof(AbsolutePathHandler));

            var m = typeof(AbsolutePathHandler).GetMethod("MethodWithStringParamEnd");
            var info = cache.Match("GET", $"/foo/bar/baz/bad", out object param);

            Assert.IsNull(info);
        }

        [TestMethod]
        public void TestIntParameter()
        {
            RequestMethodCache cache = new RequestMethodCache(null);

            cache.AddType(typeof(AbsolutePathHandler));

            var p = 1234;

            var m = typeof(AbsolutePathHandler).GetMethod("MethodWithIntParamEnd");
            var info = cache.Match("GET", $"/foo/int/{p}", out object param);

            Assert.AreEqual(m, info.Method);
            Assert.AreEqual(p, param);
        }

        [TestMethod]
        public void TestGuidParameter()
        {
            RequestMethodCache cache = new RequestMethodCache(null);

            cache.AddType(typeof(AbsolutePathHandler));

            var p = Guid.NewGuid();

            var m = typeof(AbsolutePathHandler).GetMethod("MethodWithGuidParamEnd");
            var info = cache.Match("GET", $"/foo/gid/{p}", out object param);

            Assert.AreEqual(m, info.Method);
            Assert.AreEqual(p, param);
        }
    }
}
