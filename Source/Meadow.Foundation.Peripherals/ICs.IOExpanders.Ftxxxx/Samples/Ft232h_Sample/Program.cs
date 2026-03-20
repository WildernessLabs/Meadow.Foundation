// See https://aka.ms/new-console-template for more information
using Meadow;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.ICs.ADC;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Foundation.Sensors.Light;
using Meadow.Hardware;
using Meadow.Peripherals.Displays;
using System.Diagnostics;

Console.WriteLine("HELLO FROM THE WILDERNESS FT232H DRIVER!");

FtdiExpander? expander = null;

// Make sure we clean up resources when the program is terminated
Console.CancelKeyPress += (s, e) =>
{
    Console.WriteLine("\nCleaning up...");
    expander?.Dispose();
    e.Cancel = false; // Allow the process to terminate
};

// Main logic.
try 
{
    var count = FtdiExpanderCollection.Devices.Count;
    Console.WriteLine($"Found {count} devices");

    if (count == 0)
    {
        Console.WriteLine("No devices found.");
        return;
    }

    expander = FtdiExpanderCollection.Devices[0];

    // These are various tests you can run to try different things:
    //await TestSPIDisplay(expander);
    //await TestGpio(expander);
    await TestI2C(expander);
    //await TestSPI(expander);
    //await TestRfid(expander);
}
catch (Exception ex)
{
    if (ex.Message.Contains("FT_DEVICE_NOT_OPENED"))
    {
        Console.WriteLine("\n[!] CRITICAL ERROR: The device handle is stuck in an OPEN state.");
        Console.WriteLine("    This usually happens if the application was terminated abruptly.");
        Console.WriteLine("    ACTION REQUIRED: Unplug the USB device and plug it back in to reset the driver.\n");
    }
    else
    {
        Console.WriteLine($"An error occurred: {ex.Message}");
        Console.WriteLine(ex.StackTrace);
    }
}
finally
{
    Console.WriteLine("Cleaning up...");
    expander?.Dispose();
}

async Task TestSPIDisplay(FtdiExpander expander)
{
    var display = new St7789
        (
            spiBus: expander.CreateSpiBus(),
            chipSelectPin: expander.Pins.D0,
            dcPin: expander.Pins.D1,
            resetPin: null,
            135, 240
        );

    var microGraphics = new MicroGraphics(display)
    {
        CurrentFont = new Font12x16(),
        Rotation = RotationType._270Degrees
    };

    microGraphics.Clear();
    microGraphics.DrawText(0, 0, "Loading Menu");
    microGraphics.Show();

    Console.WriteLine("Display initialized. Entering loop...");

    while (true)
    {
        Console.WriteLine("Sleeping...");

        await Task.Delay(1000);
    }
}

async Task TestSPI(FtdiExpander expander)
{
    var mcp = new Mcp3201(
        expander.CreateSpiBus(),
        expander.Pins.C1.CreateDigitalOutputPort());

    var inp = mcp.CreateAnalogInputPort();

    while (true)
    {
        Debug.WriteLine("Reading...");
        try
        {
            var t = await inp.Read();
            Debug.WriteLine($"{t.Volts} V");
        }
        catch
        {
        }

        await Task.Delay(1000);
    }
}

async Task TestI2C(FtdiExpander expander)
{
    Console.WriteLine("Initializing sensor...");
    var sensor = new Veml7700(expander.CreateI2cBus());

    while (true)
    {
        Console.WriteLine("Reading...");
        try
        {
            var t = await sensor.Read();
            Console.WriteLine($"{t.Lux} lux");
        }
        catch
        {
        }

        await Task.Delay(1000);
    }
}

async Task TestGpio(IEnumerable<FtdiExpander> expanders)
{

    var outputs = new List<IDigitalOutputPort>();

    foreach (var expander in expanders)
    {
        outputs.Add(expander.CreateDigitalOutputPort(expander.Pins.C0));
        outputs.Add(expander.CreateDigitalOutputPort(expander.Pins.C1));
        outputs.Add(expander.CreateDigitalOutputPort(expander.Pins.C2));
        outputs.Add(expander.CreateDigitalOutputPort(expander.Pins.C3));
        outputs.Add(expander.CreateDigitalOutputPort(expander.Pins.C4));
        outputs.Add(expander.CreateDigitalOutputPort(expander.Pins.C5));
        outputs.Add(expander.CreateDigitalOutputPort(expander.Pins.C6));
        outputs.Add(expander.CreateDigitalOutputPort(expander.Pins.C7));
        outputs.Add(expander.CreateDigitalOutputPort(expander.Pins.D3));
        outputs.Add(expander.CreateDigitalOutputPort(expander.Pins.D4));
        outputs.Add(expander.CreateDigitalOutputPort(expander.Pins.D5));
        outputs.Add(expander.CreateDigitalOutputPort(expander.Pins.D6));
        outputs.Add(expander.CreateDigitalOutputPort(expander.Pins.D7));
    }

    var s = false;

    while (true)
    {

        for (var i = 0; i < outputs.Count; i++)
        {
            var setTo = (i % 2 == 0) ? s : !s;
            outputs[i].State = setTo;
        }

        await Task.Delay(1000);
        s = !s;
    }
}

// async Task TestRfid(FtdiExpander expander)
// {
//    var sensor = new Mfrc522(
//        spiBus: expander.CreateSpiBus(),
//        chipSelectPort: expander.Pins.C0.CreateDigitalOutputPort(true),
//        resetPort: expander.Pins.C1.CreateDigitalOutputPort(false)
//    );

//    var result = sensor.SelfTest();
// }