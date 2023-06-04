using System.Net;
using System.Net.Sockets;
using System.Text;

const int PORT = 5100;
const char DELIMITER = '\t';

Console.WriteLine("Meadow UDP Log Client");

UdpClient udpClient = new UdpClient();
udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, PORT));

var from = new IPEndPoint(0, 0);
while (true)
{
    var recvBuffer = udpClient.Receive(ref from);
    var payload = Encoding.UTF8.GetString(recvBuffer);
    var parts = payload.Split(new char[] { DELIMITER });
    Console.WriteLine($"{parts[0]}: {parts[1].Trim()}");
}
