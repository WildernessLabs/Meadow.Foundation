using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Transceivers;
using System;
using System.Threading.Tasks;

namespace WioE5_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        private IModem? modem;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initializing GroveWioE5 LoRa Module...");

            // Create modem instance - update COM port as needed
            modem = ModemService.Identify(Device.PlatformOS.GetSerialPortName("COM4"));

            // Subscribe to events
            if (modem is WioE5Modem wioModem)
            {
                wioModem.Joined += (s, e) =>
                    Resolver.Log.Info("Successfully joined LoRaWAN network!");

                wioModem.JoinFailed += (s, e) =>
                    Resolver.Log.Error("Failed to join LoRaWAN network");

                wioModem.DataReceived += (s, e) =>
                    Resolver.Log.Info($"Received data on port {e.Port}: {e.Data}");
            }

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            try
            {
                // Test 1: Get firmware version
                Resolver.Log.Info("Test 1: Getting firmware version...");
                var version = await modem!.GetFirmwareVersion();
                Resolver.Log.Info($"Firmware Version: {version}");

                // Test 2: Set work mode to OTAA
                Resolver.Log.Info("Test 2: Setting work mode to LWOTAA...");
                await modem.SetWorkMode(WorkMode.LWOTTA);
                Resolver.Log.Info("Work mode set successfully");

                // Test 3: Join network (REPLACE WITH YOUR CREDENTIALS)
                Resolver.Log.Info("Test 3: Joining LoRaWAN network...");
                await modem.JoinNetwork(
                    devEui: "YOUR_DEV_EUI_HERE",
                    appEui: "YOUR_APP_EUI_HERE",
                    appKey: "YOUR_APP_KEY_HERE"
                );

                // Wait for join event
                Resolver.Log.Info("Waiting for join confirmation...");
                await Task.Delay(TimeSpan.FromSeconds(30));

                // Test 4: Send test message
                Resolver.Log.Info("Test 4: Sending test message...");
                var testData = System.Text.Encoding.UTF8.GetBytes("Hello from WioE5!");
                await modem.SendData(testData, confirm: false);
                Resolver.Log.Info("Test message sent");

                Resolver.Log.Info("All tests complete!");
            }
            catch (Exception ex)
            {
                Resolver.Log.Error($"Error during testing: {ex.Message}");
            }
        }
    }
}
