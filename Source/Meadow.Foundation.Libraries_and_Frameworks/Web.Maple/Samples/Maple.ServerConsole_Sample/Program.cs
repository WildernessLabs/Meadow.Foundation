using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Meadow.Foundation.Web.Maple.Server;
using Meadow.Foundation.Web.Maple;

namespace Maple.ServerBasic_Sample
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            MapleServer server = new MapleServer(
                IPAddress.Parse("127.0.0.1"),
                // OR:
                //GetLocalIP(),
                advertise: true,
                processMode: RequestProcessMode.Parallel
                );

            server.Start();

            Console.WriteLine($"Server is listening on {server.IPAddress}");
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();

            server.Stop();

            Console.WriteLine("Goodbye.");
        }

        static IPAddress GetLocalIP()
        {
            IPAddress localIP;
            using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0)) {
                socket.Connect("8.8.8.8", 65530);
                IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                localIP = endPoint.Address;
            }
            return localIP;
        }
    }
}
