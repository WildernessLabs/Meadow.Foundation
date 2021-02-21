using System;
using System.Collections.Generic;
using Meadow.Foundation.Maple;
using Meadow.Foundation.Maple.Routing;

namespace Maple.Server_SimpleMeadowSample.RequestHandlers
{
    public class HelloRequestHandler : RequestHandlerBase
    {
        public HelloRequestHandler() { }

        [HttpGet]
        public void Hello()
        {
            Console.WriteLine("GET::Hello");
            this.Context.Response.ContentType = ContentTypes.Application_Text;
            this.Context.Response.StatusCode = 200;
            this.Send("hello world").Wait();
        }


        [HttpGet]
        public void JsonSample()
        {
            Console.WriteLine("GET::JsonSample");

            List<string> names = new List<string> {
                "johnny",
                "deedee",
                "joey",
                "tommy"
            };


            this.Context.Response.ContentType = ContentTypes.Application_Json;
            this.Context.Response.StatusCode = 200;
            this.Send(names).Wait();
        }

    }
}
