using System;
using System.Collections.Generic;
using Meadow.Foundation.Web.Maple;
using Meadow.Foundation.Web.Maple.Routing;

namespace Maple.ServerSimpleMeadow_Sample.RequestHandlers
{
    public class HelloRequestHandler : RequestHandlerBase
    {
        [HttpGet("/getjsonsample")]
        public IActionResult GetJsonSample()
        {
            Console.WriteLine("GET::GetJsonSample");

            List<string> data = new List<string> {
                "johnny",
                "deedee",
                "joey",
                "tommy"
            };

            return new JsonResult(data);
        }

        [HttpGet("/getwithparameter")]
        public IActionResult GetWithParameter()
        {
            Console.WriteLine("GET::GetWithParameter");

            string name = QueryString["name"];

            Console.WriteLine($"name:{name}");
            return new OkObjectResult($"hello, {name}");
        }

        [HttpGet("/getwithparameters")]
        public IActionResult GetWithParameters()
        {
            Console.WriteLine("GET::GetWithParameters");

            string name = QueryString["name"];
            string nickname = QueryString["nickname"];

            Console.WriteLine($"name:{name}, nickname:{nickname}");
            return new OkObjectResult($"hello, {name} aka {nickname}");
        }

        [HttpPost("/posttest")]
        public IActionResult PostTest()
        {
            Console.WriteLine("GET::PostTest");

            string name = Body;

            Console.WriteLine($"name:{name}");

            return new OkResult();
        }
    }
}