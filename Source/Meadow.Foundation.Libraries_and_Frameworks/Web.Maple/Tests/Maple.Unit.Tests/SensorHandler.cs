using Meadow.Foundation.Web.Maple.Server;
using Meadow.Foundation.Web.Maple.Server.Routing;
using System.Net;

namespace Maple.Unit.Tests
{
    public class SensorData
    {
        public string Location { get; set; }
        public double Temperature { get; set; }
    }

    public class SensorHandler : IRequestHandler
    {
        public HttpListenerContext Context { get; set; }

        public bool IsReusable => true;

        [HttpPut]
        public void PutContent([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Disallow)]SensorData data)
        {

        }

        [HttpPost]
        public void PostContent([FromBody] SensorData data)
        {

        }

        public void Dispose()
        {
        }
    }
}
