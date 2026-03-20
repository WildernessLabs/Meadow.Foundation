using Meadow;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.MicroLayout;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Foundation.Sensors.Hid;

namespace Charts_Sample;

public class MeadowApp : App<Desktop>
{
    private DisplayScreen? screen;

    private int _currentLayoutIndex = 0;
    private readonly List<ILayout> _layouts = new();

    public override Task Run()
    {
        var labelFont = new Font12x20();

        screen = new DisplayScreen(Device.Display!);
        screen.BackgroundColor = Color.AntiqueWhite;

        var keyboard = new Keyboard();

        _layouts.Add(new LineChartLayout(screen.Width, screen.Height));

        var right = new PushButton(keyboard.Pins.Right);
        right.PressStarted += (s, e) =>
        {
            _currentLayoutIndex = (_currentLayoutIndex + 1) % _layouts.Count;
            ShowCurrentLayout();
        };
        var left = new PushButton(keyboard.Pins.Left);
        left.PressStarted += (s, e) =>
        {
            _currentLayoutIndex = (_currentLayoutIndex - 1 + _layouts.Count) % _layouts.Count;
            ShowCurrentLayout();
        };

        foreach (var layout in _layouts)
        {
            screen.Controls.Add(layout);
        }

        ShowCurrentLayout();

        // NOTE: this will not return until the display is closed
        ExecutePlatformDisplayRunner();

        return base.Run();
    }

    private void ShowCurrentLayout()
    {
        for (var i = 0; i < _layouts.Count; i++)
        {
            if (i == _currentLayoutIndex)
            {
                _layouts[i].IsVisible = true;
            }
            else
            {
                _layouts[i].IsVisible = false;
            }
        }
    }

    private void ExecutePlatformDisplayRunner()
    {
        if (Device.Display is SilkDisplay sd)
        {
            sd.Resize(sd.Width, sd.Height, 3);
            sd.Run();
        }
        MeadowOS.TerminateRun();
        Environment.Exit(0);
    }
}