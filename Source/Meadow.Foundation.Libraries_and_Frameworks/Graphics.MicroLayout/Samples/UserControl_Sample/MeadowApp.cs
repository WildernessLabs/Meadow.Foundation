using Meadow;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.MicroLayout;
using Meadow.Peripherals.Displays;

namespace HMI_Sample;

public class MeadowApp : App<Desktop>
{
    private DisplayScreen? screen;
    private const int PointsPerSeries = 50;

    public override Task Run()
    {
        var labelFont = new Font12x20();

        if (Device.Display is IResizablePixelDisplay d)
        {
            d.Resize(480, 800, 1);
        }

        screen = new DisplayScreen(Device.Display!);
        screen.BackgroundColor = Color.AntiqueWhite;

        var container = new AbsoluteLayout(screen.Width / 2, screen.Height / 2, screen.Width / 2, screen.Height / 2)
        {
            BackgroundColor = Color.LightGray
        };

        var attribute = new AttributeLayout("Strength", 10, 10, 16, 1);
        var save = new SaveLayout("Fort", "+1", 10, attribute.Bottom + 2);
        container.Controls.Add(attribute, save);

        screen.Controls.Add(container);

        // NOTE: this will not return until the display is closed
        ExecutePlatformDisplayRunner();

        return base.Run();
    }

    private void ExecutePlatformDisplayRunner()
    {
        if (Device.Display is SilkDisplay sd)
        {
            sd.Run();
        }
        MeadowOS.TerminateRun();
        Environment.Exit(0);
    }
}