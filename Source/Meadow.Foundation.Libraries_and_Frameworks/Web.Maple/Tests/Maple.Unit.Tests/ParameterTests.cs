using Meadow.Foundation.Web.Maple.Server;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Maple.Unit.Tests
{
    [TestClass]
    public class ParameterTests
    {
        [TestMethod]
        public void TestFromBody()
        {
            var cache = new RequestMethodCache(null);

            cache.AddType(typeof(SensorHandler));

            var postInfo = cache.Match("POST", $"/sensor", out object postParam);
            var putInfo = cache.Match("PUT", $"/sensor", out object putParam);

            Assert.IsNotNull(postInfo);
            Assert.IsNotNull(putInfo);
        }
    }
}
