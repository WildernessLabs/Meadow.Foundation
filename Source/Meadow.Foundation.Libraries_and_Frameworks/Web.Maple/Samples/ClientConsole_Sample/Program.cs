using Meadow.Foundation.Web.Maple.Client;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace Maple.ClientConsole_Sample
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            App app = new App();
            Console.WriteLine("Exiting.");
        }
    }

    public class App
    {
        MapleClient mapleClient;

        public App()
        {
            Initialize();

            Console.WriteLine("Listening for server, press any key to exit.");

            //_ = GetTest();
            _ = PostTest();

            var key = Console.ReadKey();
        }

        void Initialize()
        {
            mapleClient = new MapleClient();

            mapleClient.Servers.CollectionChanged += ServersCollectionChanged;

            _ = mapleClient.StartScanningForAdvertisingServers();
        }

        async Task GetTest()
        {
            var data = await mapleClient.GetAsync("127.0.0.1", 5417, "Hello", "Name", "Meadow 5.1");

            Console.WriteLine($"GET: {data}");

            var parameters = new Dictionary<string, string>();
            parameters.Add("Name", "Meadow");
            parameters.Add("NickName", "Tiny .NET");

            data = await mapleClient.GetAsync("127.0.0.1", 5417, "Hello", parameters);

            Console.WriteLine($"GET: {data}");
        }

        async Task PostTest() 
        {
            var data = await mapleClient.PostAsync("127.0.0.1", 5417, "HelloPost", "Meadow");
            Console.WriteLine($"POST: {data}");
        }

        void ServersCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    //TODO: should be able to do this with linq, no?
                    var servers = new List<string>();

                    foreach (var server in e.NewItems)
                    {
                        servers.Add($"'{((ServerModel)server).Name}' @ ip:[{((ServerModel)server).IpAddress}]");
                    }
                    Console.WriteLine($"New server(s) found: {string.Join(", ", servers)}");
                    break;
            }
        }
    }
}