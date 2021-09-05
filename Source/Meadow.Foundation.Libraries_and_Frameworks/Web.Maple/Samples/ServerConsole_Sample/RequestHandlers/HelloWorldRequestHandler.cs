using System;
using System.Collections.Generic;
using Meadow.Foundation.Web.Maple.Server;
using Meadow.Foundation.Web.Maple.Server.Routing;

namespace Maple.ServerBasic_Sample.RequestHandlers
{
    public class HelloRequestHandler : RequestHandlerBase
    {
        public override bool IsReusable => true;

        public HelloRequestHandler()
        {
            Console.WriteLine("HelloRequestHandler created");
        }

        [HttpGet]
        public OkObjectResult Hello()
        {
            Console.WriteLine("GET::Hello");

            //example of multiple params 
            if(QueryString.Count == 1)
            {
                string name = QueryString["name"];
                return new OkObjectResult($"hello, {name}");
            }
            else //assume more than one -- could definitely use more defensive coding
            {
                string name = QueryString["name"];
                string nickname = QueryString["nickname"];
                return new OkObjectResult($"hello, {name} aka {nickname}");
            }
        }

        [HttpGet]
        public IActionResult JsonSample()
        {
            Console.WriteLine("GET::JsonSample");

            var names = new List<string> {
                "johnny",
                "deedee",
                "joey",
                "tommy"
            };

            return new JsonResult(names);
        }

        [HttpPost("hello")]
        public IActionResult HelloPost() 
        {
            string name = Body;

            Console.WriteLine($"/HelloPost - name:{name}");

            return new OkResult();
        }
    }
}