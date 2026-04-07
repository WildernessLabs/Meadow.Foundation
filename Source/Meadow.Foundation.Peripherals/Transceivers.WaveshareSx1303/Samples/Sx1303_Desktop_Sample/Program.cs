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
        try
        {
            modem.InitializeRadios(
                freqHzRadioA: 902_300_000,
                freqHzRadioB: 903_900_000,
                clockSource: Sx1303.ClockSource.RadioA);
            Console.WriteLine("Radio init complete.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Radio init FAILED: {ex.Message}");
            Console.WriteLine("Continuing to OTP read anyway (clock may not be running)...");
        }

        // ---- OTP reads (should work now that clock is running) ----
        Console.WriteLine("\nReading OTP...");
        modem.ReadOtpDiagnostics(out var euiBytes, out var otpByteD0,
                                  out var byte00Ready, out var byteD0Ready);

        Console.Write("EUI (OTP 0x00-0x07):");
        foreach (var b in euiBytes) Console.Write($" {b:X2}");
        Console.WriteLine($"  (FSM_READY for [0x00] = {byte00Ready})");
        Console.WriteLine($"OTP byte[0xD0] = 0x{otpByteD0:X2}  (FSM_READY = {byteD0Ready})");

        var model = modem.GetModelId();
        Console.WriteLine($"Model ID: {model} (0x{(byte)model:X2})");

        Console.WriteLine("\nDone.");
    }
}
