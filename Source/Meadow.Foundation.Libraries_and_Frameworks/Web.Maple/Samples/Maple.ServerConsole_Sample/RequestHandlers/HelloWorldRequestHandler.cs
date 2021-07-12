using System;
using System.Collections.Generic;
using Meadow.Foundation.Web.Maple.Server;
using Meadow.Foundation.Web.Maple.Server.Routing;

namespace Maple.ServerBasic_Sample.RequestHandlers
{
    public class HelloRequestHandler : RequestHandlerBase
    {
        [HttpGet]
        public void Hello()
        {
            Console.WriteLine("GET::Hello");

            //example of multiple params 
            if(QueryString.Count == 1)
            {
                string name = QueryString["name"];

                Context.Response.ContentType = ContentTypes.Application_Text;
                Context.Response.StatusCode = 200;
                Send($"hello, {name}").Wait();
            }
            else //assume more than one -- could definitely use more defensive coding
            {
                string name = QueryString["name"];
                string nickname = QueryString["nickname"];

                Context.Response.ContentType = ContentTypes.Application_Text;
                Context.Response.StatusCode = 200;
                Send($"hello, {name} aka {nickname}").Wait();
            }
        }

        [HttpGet]
        public void JsonSample()
        {
            Console.WriteLine("GET::JsonSample");

            var names = new List<string> {
                "johnny",
                "deedee",
                "joey",
                "tommy"
            };

            Context.Response.ContentType = ContentTypes.Application_Json;
            Context.Response.StatusCode = 200;
            Send(names).Wait();
        }

        [HttpPost]
        public void HelloPost() 
        {
            string name = Body;

            Console.WriteLine($"/HelloPost - name:{name}");

            Context.Response.ContentType = ContentTypes.Application_Text;
            Context.Response.StatusCode = 200;
            Send($"hello, {name}").Wait();
        }
    }
}