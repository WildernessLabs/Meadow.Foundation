using System;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Maple.Server_UDP_Listener
{
    class MainClass
    {
        const int MAPLE_SERVER_BROADCASTPORT = 17756;

        static void Main(string[] args)
        {
            UdpClient udpClient = new UdpClient(MAPLE_SERVER_BROADCASTPORT);
            IPEndPoint RemoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);

            while (true) {
                Byte[] receiveBytes = udpClient.Receive(ref RemoteIpEndPoint);
                string returnData = Encoding.ASCII.GetString(receiveBytes);
                Console.WriteLine(returnData);
            }

        }
    }
}
