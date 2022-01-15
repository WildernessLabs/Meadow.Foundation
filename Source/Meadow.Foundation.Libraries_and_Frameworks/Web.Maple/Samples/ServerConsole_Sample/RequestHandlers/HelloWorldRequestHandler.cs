using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
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

        [HttpGet("/")]
        public IActionResult GetRoot()
        {
            return this.Ok("Root Request");
        }

        [HttpGet("/hello")]
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

        [HttpGet("/JsonSample")]
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

        [HttpPost("/hello")]
        public IActionResult HelloPost() 
        {
            string name = Body;

            Console.WriteLine($"/HelloPost - name:{name}");

            return new OkResult();
        }

        [HttpPost("/foo/{name}")]
        public IActionResult ParameterPost(string name)
        {
            Console.WriteLine($"/HelloPost - name:{name}");

            return new OkResult();
        }

        [HttpPost("/bar/{id}")]
        public IActionResult ParameterPost(int id)
        {
            Console.WriteLine($"/HelloPost - id:{id}");

            return new OkResult();
        }

        [HttpPost("/file/{name}")]
        public IActionResult FilePost(string name)
        {
            var buffer = new byte[4096];

            var fi = new FileInfo(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), name));

            using (var reader = new BinaryReader(Context.Request.InputStream))
            using (var writer = fi.OpenWrite())
            {
                int read = 0;
                do
                {
                    read = reader.Read(buffer, 0, buffer.Length);
                    writer.Write(buffer, 0, read);
                } while (read > 0);
            }

            return new OkResult();
        }
    }
}