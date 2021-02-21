using System;
using System.Threading;
using Meadow.Foundation.Maple;

namespace Maple.Server_BasicSample
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            //MapleServer server = new MapleServer(null);
            MapleServer server = new MapleServer(System.Net.IPAddress.Parse("192.168.0.41"));

            // start maple server and send name broadcast address
            //server.Start("my server", Initializer.CurrentNetworkInterface.IPAddress);
            server.Start();
            
            Thread.Sleep(Timeout.Infinite);

        }
    }
}
