using Meadow.Devices;
using Meadow.Foundation.Graphics;
using Meadow.Hardware;
using Meadow.Units;

namespace Meadow.Foundation.Displays.UI;

public interface IDisplayService
{
    IGraphicsDisplay Display { get; }
}

public class Ili9341DisplayService : IDisplayService
{
    public IGraphicsDisplay Display { get; }

    public Ili9341DisplayService(Ili9481 display)
    {
        Display = display;
    }

    public Ili9341DisplayService(
        ISpiBus spiBus,
        IDigitalOutputPort chipSelectPort,
        IDigitalOutputPort dcPort,
        IDigitalOutputPort resetPort)
    {
        Display = new Ili9341(
                   spiBus: spiBus,
                   chipSelectPort: chipSelectPort,
                   dataCommandPort: dcPort,
                   resetPort: resetPort,
                   width: 240, height: 320,
                   colorMode: ColorMode.Format16bppRgb565)
        {
            SpiBusMode = SpiClockConfiguration.Mode.Mode3,
            SpiBusSpeed = new Frequency(48000, Frequency.UnitType.Kilohertz)
        };
    }
}

public class LayoutService
{
    private DisplayScreen _screen;

    public LayoutService(IGraphicsDisplay display)
    {
        _screen = new DisplayScreen(display, Meadow.Foundation.Graphics.RotationType._270Degrees);
        _screen.BackgroundColor = Color.Black;
    }
}

public class MeadowApp : App<F7CoreComputeV2>
{
    private DisplayScreen _screen;

    public static async Task Main(string[] args)
    {
        await MeadowOS.Start(args);
    }

    public override Task Initialize()
    {
        var spiBus = Device.CreateSpiBus(
                    Device.Pins.SCK,
                    Device.Pins.COPI,
                    Device.Pins.CIPO,
                    new Frequency(48000, Frequency.UnitType.Kilohertz));

        var displayService = new Ili9341DisplayService(
            spiBus,
            );
        Resolver.Services.Add<IDisplayService>(displayService);
        Resolver.Services.Add<IDisplayService>(new LayoutService(displayService.Display));

        return base.Initialize();
    }

    public override Task Run()
    {
        Text();

        return base.Run();
    }

    public void Text()
    {
        var label = new DisplayLabel(0, 0, _screen.Width, _screen.Height);
        label.HorizontalAlignment = Meadow.Foundation.Graphics.HorizontalAlignment.Center;
        label.VerticalAlignment = Meadow.Foundation.Graphics.VerticalAlignment.Center;
        label.TextColor = Color.White;
        label.Text = "HELLO";

        _screen.Controls.Add(label);

    }

    public void TextOnBox()
    {
        var box = new DisplayBox(0, 0, _screen.Width / 4, _screen.Height);
        box.ForeColor = Color.Red;
        var label = new DisplayLabel(0, 0, _screen.Width / 4, _screen.Height);
        label.HorizontalAlignment = Meadow.Foundation.Graphics.HorizontalAlignment.Center;
        label.VerticalAlignment = Meadow.Foundation.Graphics.VerticalAlignment.Center;
        label.TextColor = Color.Black;
        label.Text = "Meadow";

        _screen.Controls.Add(box, label);

        while (true)
        {
            Thread.Sleep(1000);
            var temp = box.ForeColor;
            box.ForeColor = label.TextColor;
            label.TextColor = temp;
        }
    }

    public void Sweep()
    {
        var box = new DisplayBox(0, 0, _screen.Width / 4, _screen.Height);
        box.ForeColor = Color.Red;

        _screen.Controls.Add(box);

        var direction = 1;
        var speed = 1;

        while (true)
        {
            var left = box.Left + (speed * direction);

            box.Left = left;

            if ((box.Right >= _screen.Width) || box.Left <= 0)
            {
                direction *= -1;
            }

            Thread.Sleep(50);
        }

    }

}
