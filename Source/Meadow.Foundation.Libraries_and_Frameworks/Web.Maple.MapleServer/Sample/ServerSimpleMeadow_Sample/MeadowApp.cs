using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Web.Maple;
using Meadow.Gateway.WiFi;
using Meadow.Hardware;
using System;
using System.Threading.Tasks;

namespace Maple.ServerSimpleMeadow_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        MapleServer server;

        public override async Task Initialize()
        {
            Console.WriteLine("Initialize...");

            // connnect to the wifi network.
            var wifi = Device.NetworkAdapters.Primary<IWiFiNetworkAdapter>();
            Console.WriteLine($"Connecting to WiFi Network {Secrets.WIFI_NAME}");
            var connectionResult = await wifi.Connect(Secrets.WIFI_NAME, Secrets.WIFI_PASSWORD);

            if(connectionResult.ConnectionStatus != ConnectionStatus.Success)
            {
                throw new Exception($"Cannot connect to network: {connectionResult.ConnectionStatus}");
            }
            Console.WriteLine($"Connected. IP: {wifi.IpAddress}");

            // create our maple web server
            server = new MapleServer(
                wifi.IpAddress,
                advertise: true,
                processMode: RequestProcessMode.Parallel
                );

        }

        public override Task Run()
        {
            server.Start();

            return Task.CompletedTask;
        }
    }
}