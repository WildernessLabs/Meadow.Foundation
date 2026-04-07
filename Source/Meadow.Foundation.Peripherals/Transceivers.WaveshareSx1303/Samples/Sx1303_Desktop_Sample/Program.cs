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
        var cs = expander.CreateDigitalOutputPort(expander.Pins.C0, initialState: true);   // CS inactive (HIGH) at start
        var rst = expander.CreateDigitalOutputPort(expander.Pins.C1, initialState: false); // RST GPIO LOW = chip not in reset
        var pwr = expander.CreateDigitalOutputPort(expander.Pins.C2, initialState: false); // power off until constructor enables it
        var modem = new Sx1303(spi, cs, rst, pwr);

        // ---- Basic SPI verification ----
        var version = modem.GetVersion();
        Console.WriteLine($"VERSION: 0x{version:X2}");

        var (written, readBack) = modem.WriteVerify(0x42);
        Console.WriteLine($"Write verify: wrote 0x{written:X2}, read back 0x{readBack:X2}  " +
                          $"({(written == readBack ? "WRITES WORK" : "WRITES BROKEN")})");

        if (version != 0x12)
        {
            Console.WriteLine("ERROR: unexpected version — check wiring and power. Aborting.");
            return;
        }

        // ---- Radio initialization ----
        // US915 band: Radio A at 902.3 MHz, Radio B at 903.9 MHz
        Console.WriteLine("\nInitializing radios...");
        modem.InitializeRadios(
            freqHzRadioA: 902_300_000,
            freqHzRadioB: 903_900_000,
            clockSource: Sx1303.ClockSource.RadioA);
        Console.WriteLine("Radio init complete.");

        // ---- OTP / Model ID ----
        var model = modem.GetModelId();
        Console.WriteLine($"Model ID: {model} (0x{(byte)model:X2})");

        // ---- Channel configuration ----
        // US915 uplink channels: 8 x 125 kHz channels spread across Radio A and Radio B
        // Radio A center = 902.3 MHz, Radio B center = 903.9 MHz
        Console.WriteLine("\nConfiguring channels...");
        var channels = new Sx1303.ChannelConfig[]
        {
            new() { Enabled = true, Radio = 0, FreqOffsetHz = -400_000 },  // ch0: 901.9 MHz
            new() { Enabled = true, Radio = 0, FreqOffsetHz = -200_000 },  // ch1: 902.1 MHz
            new() { Enabled = true, Radio = 0, FreqOffsetHz =  000_000 },  // ch2: 902.3 MHz
            new() { Enabled = true, Radio = 0, FreqOffsetHz =  200_000 },  // ch3: 902.5 MHz
            new() { Enabled = true, Radio = 1, FreqOffsetHz = -400_000 },  // ch4: 903.5 MHz
            new() { Enabled = true, Radio = 1, FreqOffsetHz = -200_000 },  // ch5: 903.7 MHz
            new() { Enabled = true, Radio = 1, FreqOffsetHz =  000_000 },  // ch6: 903.9 MHz
            new() { Enabled = true, Radio = 1, FreqOffsetHz =  200_000 },  // ch7: 904.1 MHz
            new() { Enabled = false, Radio = 0, FreqOffsetHz = 0 },        // ch8: LoRa service (disabled)
            new() { Enabled = false, Radio = 0, FreqOffsetHz = 0 },        // ch9: FSK (disabled)
        };
        modem.ConfigureChannelizer(channels);
        modem.ConfigureSyncword(Sx1303.SyncwordMode.Public);
        Console.WriteLine("Channels configured.");

        // ---- Start concentrator ----
        Console.WriteLine("\nStarting concentrator (loading firmware, enabling modems)...");
        modem.StartConcentrator();
        Console.WriteLine("Concentrator running. Listening for packets...\n");

        // ---- RX loop ----
        Console.CancelKeyPress += (s, e) => { e.Cancel = false; };
        Console.WriteLine("Press Ctrl+C to stop.\n");

        int pktCount = 0;
        while (true)
        {
            var packets = modem.Receive();
            foreach (var pkt in packets)
            {
                pktCount++;
                Console.WriteLine($"[{pktCount}] CH{pkt.Channel} SF{pkt.SpreadingFactor} " +
                                  $"RSSI:{pkt.RssiSignal}dBm SNR:{pkt.SnrDb:F1}dB " +
                                  $"CRC:{(pkt.CrcError ? "ERR" : "OK")} " +
                                  $"len={pkt.Payload.Length} ts={pkt.Timestamp}us");
                Console.Write("  payload: ");
                foreach (var b in pkt.Payload) Console.Write($"{b:X2} ");
                Console.WriteLine();
            }

            await Task.Delay(100);
        }
    }
}
