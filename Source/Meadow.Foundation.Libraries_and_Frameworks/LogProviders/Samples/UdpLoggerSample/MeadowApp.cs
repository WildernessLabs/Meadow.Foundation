using Meadow;
using Meadow.Devices;
using Meadow.Hardware;
using System;
using System.Threading.Tasks;

namespace Logging
{
    public class MeadowApp : App<F7FeatherV2>
    {
        public override async Task Initialize()
        {
            Resolver.Log.Info($"Initializing...");

            // an our own logger to the system logger
            await AddUdpLogger();
        }

        public override async Task Run()
        {
            while (true)
            {
                // prefix a random number just so we can see differences
                var r = new Random();
                Resolver.Log.Info($"Log info [{r.Next(0, 1000)}]");
                await Task.Delay(1000);
            }
        }

        private async Task AddUdpLogger()
        {
            var SSID = "MyNetwork";
            var PASSCODE = "MyPasscode";

            Resolver.Log.Info("Connecting to network...");

            try
            {
                var wifi = Device.NetworkAdapters.Primary<IWiFiNetworkAdapter>();
                await wifi.Connect(SSID, PASSCODE);

                wifi.NetworkConnected += (s, e) =>
                {
                    Resolver.Log.Info("Network connected");
                    Resolver.Log.AddProvider(new UdpLogger());
                };
            }
            catch (Exception ex)
            {
                Resolver.Log.Error($"Error connecting to network: {ex.Message}");
            }

        }
    }
}