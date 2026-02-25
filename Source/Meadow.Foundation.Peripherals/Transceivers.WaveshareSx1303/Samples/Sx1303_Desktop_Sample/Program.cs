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

        var version = modem.GetVersion();
        Console.WriteLine($"VERSION: 0x{version:X2}");

        // OTP_STATUS (0x6182): bit 0 = FSM_READY, bits 7:4 = CHECKSUM_STATUS
        // The OTP FSM needs a free-running clock (normally the radio clock after
        // sx1302_radio_clock_select).  Print raw status so we can diagnose.
        var otpStatus = modem.ReadOtpDiagnostics(out var otpByte0, out var otpByteD0);
        Console.WriteLine($"OTP_STATUS: 0x{otpStatus:X2}  (FSM_READY={(otpStatus & 1)})");
        Console.WriteLine($"OTP byte[0x00] = 0x{otpByte0:X2}  (first EUI byte)");
        Console.WriteLine($"OTP byte[0xD0] = 0x{otpByteD0:X2}  (model ID raw)");

        var model = modem.GetModelId();
        Console.WriteLine($"Model ID: {model} (0x{(byte)model:X2})");

        Console.WriteLine("Done.");
    }
}