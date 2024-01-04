using Meadow;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.MicroLayout;
using Meadow.Foundation.Sensors.Camera;
using Meadow.Hardware;
using Meadow.Pinouts;
using Meadow.Units;

public class MeadowApp : App<Linux<RaspberryPi>>
{
    private Ili9341 _display;
    private DisplayScreen _screen;
    private Amg8833 _camera;
    private Box[] _pixelBoxes;

    public static async Task Main(string[] args)
    {
        await MeadowOS.Start(args);
    }

    public override Task Initialize()
    {
        Console.WriteLine("Creating Outputs");

        _display = new Ili9341(
            Device.CreateSpiBus(0, SpiClockConfiguration.Mode.Mode0, new Frequency(48000, Frequency.UnitType.Kilohertz)),
            Device.Pins.GPIO5.CreateDigitalOutputPort(), //CS
            Device.Pins.GPIO6.CreateDigitalOutputPort(), //RST
            Device.Pins.GPIO13.CreateDigitalOutputPort(), // DC
            240,
            320,
            ColorMode.Format16bppRgb565);

        /*
        _display = new Ili9341(
            Device.CreateSpiBus(0, _display.DefaultSpiBusMode, _display.DefaultSpiBusSpeed),
            Device.Pins.GPIO5, //CS
            Device.Pins.GPIO6, //RST
            Device.Pins.GPIO13, // DC
            320,
            240);
        */
        var i2c = Device.CreateI2cBus();
        _camera = new Amg8833(i2c);

        CreateLayout();

        return base.Initialize();
    }

    private void CreateLayout()
    {
        _pixelBoxes = new Box[64];
        _screen = new DisplayScreen(_display);
        var x = 0;
        var y = 0;
        var boxSize = 40;
        for (var i = 0; i < _pixelBoxes.Length; i++)
        {
            _pixelBoxes[i] = new Box(x, y, boxSize, boxSize)
            {
                ForeColor = Color.Blue
            };

            _screen.Controls.Add(_pixelBoxes[i]);

            if (i % 8 == 7)
            {
                x = 0;
                y += boxSize;
            }
            else
            {
                x += boxSize;
            }
        }
    }

    public override async Task Run()
    {
        var t = 0;

        while (true)
        {
            var pixels = _camera.ReadPixels();

            if (t++ % 20 == 0)
            {
                Resolver.Log.Info($"tick {pixels.Average(p => p.Celsius)}");
            }

            _screen.BeginUpdate();

            for (var i = 0; i < pixels.Length; i++)
            {
                var color = pixels[i].Celsius switch
                {
                    < 20 => Color.Black,
                    < 22 => Color.DarkViolet,
                    < 24 => Color.DarkBlue,
                    < 26 => Color.DarkGreen,
                    < 28 => Color.DarkOrange,
                    < 30 => Color.Yellow,
                    _ => Color.White
                };

                _pixelBoxes[i].ForeColor = color;
            }

            _screen.EndUpdate();

            await Task.Delay(100);
        }
    }
}