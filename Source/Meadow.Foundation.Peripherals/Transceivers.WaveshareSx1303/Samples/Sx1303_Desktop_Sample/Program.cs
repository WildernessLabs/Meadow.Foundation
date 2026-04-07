using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Foundation.Transceivers;
using Meadow.Foundation.Transceivers.Waveshare;
using Meadow.Hardware;
using Meadow.Units;

internal class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Opening FT232...");

        var expander = FtdiExpanderCollection.Devices[0];

        // ADAFRUIT FT232H to SX130X HAT WIRING:
        // |  FT232H     |  HAT              |
        // |-------------|-------------------|
        // | [n.c.]      | pin02  5V         | <- Requires external PS
        // | GND         | pin06  GND        | <- common ground to external PS
        // | D0          | pin23  SCK        |
        // | D1          | pin19  MOSI       |
        // | D2          | pin21  MISO       |
        // | C0          | pin24  CS  (CS0)  |
        // | C1          | pin16  D23 (RST)  |
        // | C2          | pin12  D18 (PWEN) | <- Power enable - MUST be driven HIGH
        // dev note: docs suggest PWEN is required, but my testing worked without it. YMMV.

        var spi = expander.CreateSpiBus(0, 1_000_000.Hertz());
        var cs = expander.CreateDigitalOutputPort(expander.Pins.C0, initialState: true);
        var rst = expander.CreateDigitalOutputPort(expander.Pins.C1, initialState: false);
        var pwr = expander.CreateDigitalOutputPort(expander.Pins.C2, initialState: false);
        var modem = new Sx1303(spi, cs, rst, pwr);

        Console.WriteLine($"VERSION: 0x{modem.GetVersion():X2}");

        // Start the concentrator with US915 sub-band 2 (TTN default)
        var config = new GatewayConfig
        {
            ChannelPlan = Us915ChannelPlan.ForSubBand(2),
            Syncword = Sx1303.SyncwordMode.Public,
        };

        Console.WriteLine($"Starting: {config.ChannelPlan.Name}");
        modem.Start(config);

        var eui = modem.GetEui();
        Console.Write("Gateway EUI: ");
        Console.WriteLine(string.Join(":", eui.Select(b => b.ToString("X2"))));
        Console.WriteLine($"Model: {modem.GetModelId()}");
        Console.WriteLine("Listening for packets... (Ctrl+C to stop)\n");

        int pktCount = 0;
        while (true)
        {
            var packets = modem.Receive();
            foreach (var pkt in packets)
            {
                if (pkt.CrcError) continue; // skip bad packets

                pktCount++;
                Console.WriteLine($"[{pktCount}] CH{pkt.Channel} SF{pkt.SpreadingFactor} " +
                                  $"RSSI:{pkt.RssiSignal}dBm SNR:{pkt.SnrDb:F1}dB " +
                                  $"len={pkt.Payload.Length}");
                if (pkt.Payload.Length > 0)
                {
                    Console.Write("  ");
                    foreach (var b in pkt.Payload) Console.Write($"{b:X2} ");
                    Console.WriteLine();
                }
            }

            await Task.Delay(100);
        }
    }
}
