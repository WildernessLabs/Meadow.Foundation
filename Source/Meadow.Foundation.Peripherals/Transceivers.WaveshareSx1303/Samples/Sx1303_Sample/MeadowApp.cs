using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Transceivers.Waveshare;
using Meadow.Units;
using System.Threading.Tasks;

namespace Transceivers.WaveshareSx1303_Sample;

public class MeadowApp : App<F7FeatherV2>
{
    public override Task Run()
    {
        Resolver.Log.Info("Initializing SX1303...");

        var spi = Device.CreateSpiBus(Device.Pins.SCK, Device.Pins.MOSI, Device.Pins.MISO, new Frequency(1, Frequency.UnitType.Megahertz));
        var cs  = Device.CreateDigitalOutputPort(Device.Pins.D00, initialState: true);
        var rst = Device.CreateDigitalOutputPort(Device.Pins.D01, initialState: false);

        // D18 (PWEN) is not broken out on this board revision — passing null
        var modem = new Sx1303(spi, cs, rst, powerEnable: null);

        var version = modem.GetVersion();
        Resolver.Log.Info($"VERSION: 0x{version:X2} (expect 0x12 for SX1303)");

        var (written, readBack) = modem.WriteVerify();
        Resolver.Log.Info($"WriteVerify: wrote 0x{written:X2}, read back 0x{readBack:X2} — {(written == readBack ? "PASS" : "FAIL")}");

        if (version != 0x12 || written != readBack)
        {
            Resolver.Log.Warn("SPI comms not confirmed — check wiring before going further.");
            return Task.CompletedTask;
        }

        Resolver.Log.Info("SPI OK — starting concentrator (US915 sub-band 2)...");

        var config = new GatewayConfig
        {
            ChannelPlan = Us915ChannelPlan.ForSubBand(2),
            Syncword = Sx1303.SyncwordMode.Public,
        };

        modem.Start(config);

        var eui = modem.GetEui();
        Resolver.Log.Info($"Gateway EUI: {string.Join(":", System.Array.ConvertAll(eui, b => b.ToString("X2")))}");
        Resolver.Log.Info($"Model: {modem.GetModelId()}");
        Resolver.Log.Info("Listening for LoRa packets...");

        while (true)
        {
            var packets = modem.Receive();
            foreach (var pkt in packets)
            {
                if (pkt.CrcError) continue;
                Resolver.Log.Info($"CH{pkt.Channel} SF{pkt.SpreadingFactor} RSSI:{pkt.RssiSignal}dBm SNR:{pkt.SnrDb:F1}dB len={pkt.Payload.Length}");
            }
            Task.Delay(100).Wait();
        }
    }
}
