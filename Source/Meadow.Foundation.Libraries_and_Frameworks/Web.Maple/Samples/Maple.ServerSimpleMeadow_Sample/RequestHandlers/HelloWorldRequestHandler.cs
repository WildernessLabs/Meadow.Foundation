using System;
using System.Collections.Generic;
using Meadow.Foundation.Web.Maple.Server;
using Meadow.Foundation.Web.Maple.Server.Routing;

namespace Maple.ServerSimpleMeadow_Sample.RequestHandlers
{
    public class HelloRequestHandler : RequestHandlerBase
    {
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
