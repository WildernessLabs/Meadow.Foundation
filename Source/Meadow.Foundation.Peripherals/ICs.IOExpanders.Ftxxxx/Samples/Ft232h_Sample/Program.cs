// See https://aka.ms/new-console-template for more information
using Meadow;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Foundation.Sensors.Light;
using Meadow.Hardware;
using Meadow.Peripherals.Displays;
using System.Diagnostics;

Console.WriteLine("HELLO FROM THE WILDERNESS FT232H DRIVER!");

var count = FtdiExpanderCollection.Devices.Count();
var expander = FtdiExpanderCollection.Devices[0];

//await TestGpio(FtdiExpanderCollection.Devices);
//await TestI2C(FtdiExpanderCollection.Devices[0]);
//await TestSPI(FtdiExpanderCollection.Devices[0]);
await TestSPIDisplay(FtdiExpanderCollection.Devices[0]);

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

    while (true)
    {
        Debug.WriteLine("Sleeping...");

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
    var sensor = new Veml7700(expander.CreateI2cBus());

    while (true)
    {
        Debug.WriteLine("Reading...");
        try
        {
            var t = await sensor.Read();
            Debug.WriteLine($"{t.Lux} lux");
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