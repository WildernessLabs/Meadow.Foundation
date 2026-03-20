using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Meadow.Foundation.IOExpanders;

public abstract partial class T3xxx
{
    private const int UdpBroadcastPort = 1234;
    private const int IpChangeTimeoutMs = 3000;
    private const byte IpChangeCommand = 0x66;
    private const byte IpChangeAck = 0x67;

    /// <summary>
    /// Changes the IP address of a T3 module that may be on a different subnet, using UDP broadcast.
    /// </summary>
    /// <param name="localAddress">The IP address of the local network interface to bind to (must be on the target subnet).</param>
    /// <param name="currentDeviceIp">The current IP address of the T3 module.</param>
    /// <param name="newIp">The new IP address to assign to the T3 module.</param>
    /// <param name="subnetMask">The subnet mask for the new IP address.</param>
    /// <param name="gateway">The default gateway for the new IP address.</param>
    /// <param name="serialNumber">The serial number of the T3 module (used to target a specific device).</param>
    /// <param name="retries">Number of times to retry sending the command if no acknowledgement is received.</param>
    /// <returns>A task that returns <c>true</c> if the IP change was acknowledged by the device; otherwise <c>false</c>.</returns>
    public static async Task<bool> ChangeIpAddress(
        IPAddress localAddress,
        IPAddress currentDeviceIp,
        IPAddress newIp,
        IPAddress subnetMask,
        IPAddress gateway,
        int serialNumber,
        int retries = 3)
    {
        for (int attempt = 0; attempt < retries; attempt++)
        {
            if (await TrySendIpChangeCommand(localAddress, currentDeviceIp, newIp, subnetMask, gateway, serialNumber))
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Changes the IP address of this T3 module using UDP broadcast.
    /// Reads the serial number from the device via Modbus before sending the change command.
    /// Use the static overload when the device is unreachable via Modbus (e.g. on a different subnet).
    /// </summary>
    /// <param name="localAddress">The IP address of the local network interface to bind to.</param>
    /// <param name="currentDeviceIp">The current IP address of the T3 module.</param>
    /// <param name="newIp">The new IP address to assign to the T3 module.</param>
    /// <param name="subnetMask">The subnet mask for the new IP address.</param>
    /// <param name="gateway">The default gateway for the new IP address.</param>
    /// <param name="retries">Number of times to retry sending the command if no acknowledgement is received.</param>
    /// <returns>A task that returns <c>true</c> if the IP change was acknowledged by the device; otherwise <c>false</c>.</returns>
    public async Task<bool> ChangeIpAddress(
        IPAddress localAddress,
        IPAddress currentDeviceIp,
        IPAddress newIp,
        IPAddress subnetMask,
        IPAddress gateway,
        int retries = 3)
    {
        var serialNumber = await ReadSerialNumber();
        return await ChangeIpAddress(localAddress, currentDeviceIp, newIp, subnetMask, gateway, serialNumber, retries);
    }

    private static async Task<bool> TrySendIpChangeCommand(
        IPAddress localAddress,
        IPAddress currentDeviceIp,
        IPAddress newIp,
        IPAddress subnetMask,
        IPAddress gateway,
        int serialNumber)
    {
        using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);

        // Bind to the specific local NIC so the broadcast reaches the correct subnet
        socket.Bind(new IPEndPoint(localAddress, 0));

        var packet = BuildIpChangePacket(currentDeviceIp, newIp, subnetMask, gateway, serialNumber);
        var broadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, UdpBroadcastPort);

        await socket.SendToAsync(new ArraySegment<byte>(packet), SocketFlags.None, broadcastEndpoint);

        socket.ReceiveTimeout = IpChangeTimeoutMs;
        var buffer = new byte[512];
        var remoteEndpoint = (EndPoint)new IPEndPoint(IPAddress.Any, 0);

        try
        {
            var deadline = DateTime.UtcNow.AddMilliseconds(IpChangeTimeoutMs);
            while (DateTime.UtcNow < deadline)
            {
                if (socket.Poll((int)((deadline - DateTime.UtcNow).TotalMilliseconds * 1000), SelectMode.SelectRead))
                {
                    int received = socket.ReceiveFrom(buffer, ref remoteEndpoint);
                    if (received > 0 && buffer[0] == IpChangeAck)
                    {
                        return true;
                    }
                }
                else
                {
                    break;
                }
            }
        }
        catch (SocketException)
        {
            // Timeout or other socket error — return false below
        }

        return false;
    }

    private static byte[] BuildIpChangePacket(
        IPAddress currentDeviceIp,
        IPAddress newIp,
        IPAddress subnetMask,
        IPAddress gateway,
        int serialNumber)
    {
        // Packet layout (21 bytes):
        // [0]      command byte (0x66)
        // [1..4]   current device IP
        // [5..8]   new IP
        // [9..12]  subnet mask
        // [13..16] gateway
        // [17..20] serial number (little-endian)
        var packet = new byte[21];
        packet[0] = IpChangeCommand;

        CopyIpBytes(currentDeviceIp, packet, 1);
        CopyIpBytes(newIp, packet, 5);
        CopyIpBytes(subnetMask, packet, 9);
        CopyIpBytes(gateway, packet, 13);

        var snBytes = BitConverter.GetBytes(serialNumber);
        if (!BitConverter.IsLittleEndian) { Array.Reverse(snBytes); }
        Buffer.BlockCopy(snBytes, 0, packet, 17, 4);

        return packet;
    }

    private static void CopyIpBytes(IPAddress address, byte[] destination, int offset)
    {
#pragma warning disable CS0618 // Address is not obsolete for IPv4 byte extraction
        var bytes = address.GetAddressBytes();
#pragma warning restore CS0618
        Buffer.BlockCopy(bytes, 0, destination, offset, 4);
    }
}
