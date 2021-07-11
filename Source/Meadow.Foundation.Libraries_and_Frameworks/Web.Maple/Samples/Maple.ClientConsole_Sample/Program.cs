using Meadow.Foundation.Maple.Web.Client;
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

            _ = GetTest();

            var key = Console.ReadKey();
        }

        async Task GetTest()
        {
            var data = await mapleClient.GetAsync("127.0.0.1", 5417, "Hello", "Name", "Meadow 5.1");

            Console.WriteLine($"GET: {data}");
        }

        void Initialize()
        {
            mapleClient = new MapleClient();
            
            mapleClient.Servers.CollectionChanged += Servers_CollectionChanged;

            _ = mapleClient.StartScanningForAdvertisingServers();
        }

        private void Servers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
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
