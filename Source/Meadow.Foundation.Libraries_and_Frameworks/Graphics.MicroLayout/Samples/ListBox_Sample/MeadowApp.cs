using Meadow;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.MicroLayout;

namespace ListBox_Sample;

public class MeadowApp : App<Desktop>
{
    private DisplayScreen? screen;

    public override Task Run()
    {
        var labelFont = new Font16x24();

        screen = new DisplayScreen(Device.Display)
        {
            BackgroundColor = Color.Cyan
        };

        var listView = new ListBox(0, 0, screen.Width, screen.Height, labelFont);

        listView.Items.Add("Item A");
        listView.Items.Add("Item B");

        screen.Controls.Add(listView);

        Task.Run(async () =>
        {
            await Task.Delay(1000);
            listView.Items.Add("Item C");
            listView.SelectedIndex = 0;

            await Task.Delay(1000);
            listView.Items.Add("Item D");
            await Task.Delay(1000);
            listView.Items.Add("Item E");
            await Task.Delay(1000);
            listView.Items.Add("Item F");
            await Task.Delay(1000);
            listView.Items.Add("Item G");
            await Task.Delay(1000);
            listView.Items.RemoveAt(1);

        });

        // NOTE: this will not return until the display is closed
        ExecutePlatformDisplayRunner();

        return Task.CompletedTask;
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
