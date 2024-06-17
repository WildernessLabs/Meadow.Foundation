using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.MicroLayout;

namespace Meadow.Foundation.Displays.UI;

public class MeadowApp : App<Windows>
{
    private DisplayScreen? screen;
    private SilkDisplay display;

    public static async Task Main(string[] args)
    {
        await MeadowOS.Start(args);
    }

    public override async Task Run()
    {
        display = new SilkDisplay();

        var labelFont = new Font16x24();

        screen = new DisplayScreen(display)
        {
            BackgroundColor = Color.Black
        };

        var textArea = new ScrollingTextArea(0, 0, display.Width, display.Height, labelFont);

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

        ExecutePlatformDisplayRunner();
    }

    private void ExecutePlatformDisplayRunner()
    {
        display.Run();
        Environment.Exit(0);
    }
}