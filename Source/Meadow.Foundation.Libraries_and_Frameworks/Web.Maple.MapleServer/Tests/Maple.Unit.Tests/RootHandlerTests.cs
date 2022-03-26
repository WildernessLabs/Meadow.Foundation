using Meadow.Foundation.Web.Maple;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Maple.Unit.Tests
{
    [TestClass]
    public class RootHandlerTests
    {
        [TestMethod]
        public void TestRootHandlerSlash()
        {
            RequestMethodCache cache = new RequestMethodCache(null);

            cache.AddType(typeof(RootHandler));

            var info = cache.Match("GET", $"/", out object param);

            Assert.IsNotNull(info);
        }

        [TestMethod]
        public void TestRootHandlerEmptyStringToSlash()
        {
            RequestMethodCache cache = new RequestMethodCache(null);

            cache.AddType(typeof(RootHandler));

            var info = cache.Match("GET", $"", out object param);

            Assert.IsNotNull(info);
        }
    }
}
