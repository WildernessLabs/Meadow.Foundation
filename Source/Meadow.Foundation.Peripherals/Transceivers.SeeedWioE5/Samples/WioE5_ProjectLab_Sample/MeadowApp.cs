using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Transceivers;
using System.Text;
using System.Threading.Tasks;

namespace WioE5_ProjectLab_Sample
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        WioE5? radio;

        public override async Task Initialize()
        {
            Resolver.Log.Info("Initializing WioE5...");

            // Grove UART connector on Project Lab V1: com1 (RX=D13, TX=D12)
            var portName = Device.PlatformOS.GetSerialPortName("com1")!;
            radio = new WioE5(portName);

            radio.PacketReceived += (s, pkt) =>
            {
                var text = Encoding.ASCII.GetString(pkt.Payload);
                Resolver.Log.Info($"RX: \"{text}\" RSSI:{pkt.Rssi}dBm SNR:{pkt.Snr}dB");
            };

            var ok = await radio.Initialize();
            Resolver.Log.Info($"Initialize: {(ok ? "OK" : "FAILED")}");
        }

        public override async Task Run()
        {
            if (radio == null) return;

            var version = await radio.GetVersion();
            Resolver.Log.Info($"Firmware: {version}");

            await radio.SetTestMode();
            await radio.ConfigureRf(
                frequencyHz: 868_000_000,
                sf: SpreadingFactor.SF7,
                bandwidth: Bandwidth.BW125,
                powerDbm: 14);
            Resolver.Log.Info("RF configured: 868MHz SF7 BW125 14dBm");

            while (true)
            {
                Resolver.Log.Info("Sending packet...");
                var sent = await radio.SendPacket(Encoding.ASCII.GetBytes("Hello from Meadow"));
                Resolver.Log.Info($"TX: {(sent ? "DONE" : "FAILED")}");

                Resolver.Log.Info("Listening for reply...");
                await radio.StartReceiving();
                await Task.Delay(5000);
                await radio.StopReceiving();
            }
        }

        //<!=SNOP=>
    }
}
