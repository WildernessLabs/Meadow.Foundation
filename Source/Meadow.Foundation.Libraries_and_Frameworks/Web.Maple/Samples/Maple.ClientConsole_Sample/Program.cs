using System;
using System.Linq;
using System.Collections.Specialized;
using Meadow.Foundation.Maple.Client;
using System.Collections.Generic;

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

    public class App {
        MapleClient mapleClient;

        public App()
        {
            Initialize();
            Console.WriteLine("Listening for server, press any key to exit.");
            Console.ReadKey();
        }

        void Initialize()
        {
            mapleClient = new MapleClient();
            mapleClient.StartScanningForAdvertisingServers();
            mapleClient.Servers.CollectionChanged += Servers_CollectionChanged;
        }

        private void Servers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action) {
                case NotifyCollectionChangedAction.Add:
                    //TODO: should be able to do this with linq, no?
                    List<string> servers = new List<string>();
                    foreach (var server in e.NewItems) {
                        servers.Add($"'{((ServerModel)server).Name}' @ ip:[{((ServerModel)server).IpAddress}]");
                    }
                    Console.WriteLine($"New server(s) found: {string.Join(", ", servers)}");
                    break;
            }
        }
    }
}
