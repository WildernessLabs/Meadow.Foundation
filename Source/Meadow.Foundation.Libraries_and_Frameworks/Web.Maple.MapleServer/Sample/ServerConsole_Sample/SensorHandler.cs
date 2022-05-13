using Meadow.Foundation.Web.Maple;
using Meadow.Foundation.Web.Maple.Routing;

namespace Maple.ServerBasic_Sample.RequestHandlers
{
    public class SensorData
    {
        public string Location { get; set; }
        public double Temperature { get; set; }
    }

    public class SensorHandler : RequestHandlerBase
    {
        public override bool IsReusable => true;

        [HttpPut]
        public OkObjectResult PutContent([FromBody(EmptyBodyBehavior = EmptyBodyBehavior.Disallow)] SensorData data)
        {
            return new OkObjectResult($"Received PUT data for {data.Location}");
        }

        [HttpPost]
        public OkObjectResult PostContent([FromBody] SensorData data)
        {
            return new OkObjectResult($"Received POST data for {data.Location}");
        }
    }
}