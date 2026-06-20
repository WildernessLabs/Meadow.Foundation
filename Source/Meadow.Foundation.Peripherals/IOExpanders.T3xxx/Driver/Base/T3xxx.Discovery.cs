using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.IOExpanders;

public abstract partial class T3xxx
{
    internal const byte DiscoveryQueryCommand    = 100; // 0x64
    internal const byte DiscoveryResponseCommand = 101; // 0x65

    /// <summary>
    /// Discovers T3 modules on the network via UDP broadcast.
    /// </summary>
    /// <remarks>
    /// Sends a broadcast query on the specified local interface and collects all responses
    /// within the timeout window. Works across subnets when <paramref name="localAddress"/>
    /// is bound to an interface that can reach the target subnet. The broadcast is retried
    /// up to <paramref name="retries"/> times to improve reliability on lossy networks.
    /// </remarks>
    /// <param name="localAddress">
    /// IP address of the local network interface to bind to. Pass <see cref="IPAddress.Any"/>
    /// to listen on all interfaces (broadcast will still be sent, but cannot be steered to a
    /// specific subnet).
    /// </param>
    /// <param name="timeout">
    /// How long to wait for responses after each broadcast. Defaults to 3 seconds.
    /// </param>
    /// <param name="retries">
    /// Number of additional broadcast attempts if no new devices are heard. Defaults to 3.
    /// </param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    /// A list of <see cref="T3DeviceInfo"/> describing every T3 module that replied.
    /// </returns>
    public static async Task<IReadOnlyList<T3DeviceInfo>> DiscoverDevices(
        IPAddress? localAddress = null,
        TimeSpan? timeout = null,
        int retries = 3,
        CancellationToken cancellationToken = default)
    {
        var responseTimeoutMs = (int)(timeout ?? TimeSpan.FromSeconds(3)).TotalMilliseconds;
        var discovered = new List<T3DeviceInfo>();
        var seenSerials = new HashSet<int>();

        var interfaces = new List<IPAddress>();
        if (localAddress != null && !localAddress.Equals(IPAddress.Any))
        {
            interfaces.Add(localAddress);
        }
        else
        {
            foreach (var ni in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
            {
                if (ni.OperationalStatus != System.Net.NetworkInformation.OperationalStatus.Up) continue;
                if (ni.NetworkInterfaceType == System.Net.NetworkInformation.NetworkInterfaceType.Loopback) continue;

                var props = ni.GetIPProperties();
                foreach (var addr in props.UnicastAddresses)
                {
                    if (addr.Address.AddressFamily == AddressFamily.InterNetwork)
                    {
                        interfaces.Add(addr.Address);
                    }
                }
            }
        }

        if (interfaces.Count == 0) interfaces.Add(IPAddress.Any);

        foreach (var ip in interfaces)
        {
            using var socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
            
            try
            {
                socket.Bind(new IPEndPoint(ip, 0));
            }
            catch
            {
                continue;
            }

            var broadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, UdpBroadcastPort);
            var sendBuf = new byte[1024];
            sendBuf[0] = DiscoveryQueryCommand;
            int sendLen = 5;

            for (int attempt = 0; attempt < retries; attempt++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await socket.SendToAsync(new ArraySegment<byte>(sendBuf, 0, sendLen), SocketFlags.None, broadcastEndpoint);

                var recvBuf = new byte[512];
                var remoteEp = (EndPoint)new IPEndPoint(IPAddress.Any, 0);
                var deadline = DateTime.UtcNow.AddMilliseconds(responseTimeoutMs);
                bool anyThisRound = false;

                while (DateTime.UtcNow < deadline && !cancellationToken.IsCancellationRequested)
                {
                    int remainingUs = (int)((deadline - DateTime.UtcNow).TotalMilliseconds * 1000);
                    if (remainingUs <= 0) break;

                    if (!socket.Poll(remainingUs, SelectMode.SelectRead)) break;

                    int received = socket.ReceiveFrom(recvBuf, ref remoteEp);
                    if (received <= 0) continue;

                    var info = T3DeviceInfo.FromResponsePacket(recvBuf, received);
                    if (info == null) continue;

                    if (seenSerials.Add(info.SerialNumber))
                    {
                        discovered.Add(info);
                        anyThisRound = true;

                        var ipBytes = info.IpAddress.GetAddressBytes();
                        Buffer.BlockCopy(ipBytes, 0, sendBuf, sendLen - 4, 4);
                        sendBuf[sendLen] = 0;
                        sendBuf[sendLen + 1] = 0;
                        sendBuf[sendLen + 2] = 0;
                        sendBuf[sendLen + 3] = 0;
                        sendLen += 4;
                    }
                }

                if (anyThisRound) attempt = 0;
                else break;
            }
        }

        return discovered;
    }
}
