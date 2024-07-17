using Meadow;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.MicroLayout;

namespace TextArea_Sample;

public class MeadowApp : App<Desktop>
{
    private DisplayScreen? screen;

    public override async Task Run()
    {
        var labelFont = new Font16x24();

        screen = new DisplayScreen(Device.Display)
        {
            BackgroundColor = Color.Black
        };

        var textArea = new ScrollingTextArea(0, 0, screen.Width, screen.Height, labelFont);

        screen.Controls.Add(textArea);

        var tick = 0;
        Color? color;

        _ = Task.Run(() =>
        {
            while (true)
            {
                tick++;
                color = tick % 5 == 0 ? Color.Red : null;

                textArea.Add($"{DateTime.UtcNow:HH:mm:ss} TestItem {tick}", color);
                Thread.Sleep(1000);
            };
        });

        // NOTE: this will not return until the display is closed
        ExecutePlatformDisplayRunner();
    }

    private void ExecutePlatformDisplayRunner()
    {
        if (Device.Display is SilkDisplay sd)
        {
            sd.Run();
        }
        MeadowOS.TerminateRun();
        System.Environment.Exit(0);
    }
}