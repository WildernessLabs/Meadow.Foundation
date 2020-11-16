using System;
using System.IO;
using System.Reflection;
using System.Text.Json;

namespace JsonTestCore
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var data = LoadResource("menu.json");

            if (data != null ) { Console.WriteLine("Json loaded"); }

            var menu = System.Text.Json.JsonDocument.Parse(data);

            Console.WriteLine("Parsed to JDocument");

            var rt = menu.RootElement.GetProperty("menu");

            //  var items = rt.EnumerateArray();

            foreach (var node in rt.EnumerateArray())
            {
                string text, command = null, id = null, type = null, value = null;

                JsonElement el;

                text = node.GetProperty("text").ToString();

                if (node.TryGetProperty("id", out el))
                {
                    id = el.ToString();
                }
                if (node.TryGetProperty("command", out el))
                {
                    command = el.ToString();
                }
                if (node.TryGetProperty("type", out el))
                {
                    type = el.ToString();
                }
                if (node.TryGetProperty("value", out el))
                {
                    value = el.ToString();
                }

                Console.WriteLine($"{text}, {id}, {command}, {type}, {value}");
            }

            /*
            foreach (var item in items)
            {
                JsonElement el;

                Console.WriteLine(item.GetProperty("text").ToString());

             //   Console.WriteLine("Found item:");

                if(item.TryGetProperty("sub", out el))
                {
                 //   Console.WriteLine("Found sub menu: " + el.ToString());
                }

                // Console.WriteLine(item.ToString());
                /* if (item.TryGetProperty("sub", out el));
                 {
                     if (el.)
                     {
                         Console.WriteLine("found more");
                     } 
                 } 
            } */
            
            Console.WriteLine("Done");
        }

        static byte[] LoadResource(string filename)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"JsonTestCore.{filename}";

            using (Stream stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }
    }
}
