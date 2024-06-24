using Meadow;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.MicroLayout;

namespace Meadow.Foundation.Displays.UI;

public class MeadowApp : App<Desktop>
{
    private DisplayScreen? screen;

    public static async Task Main(string[] args)
    {
        await MeadowOS.Start(args);
    }

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

        return Task.CompletedTask;
    }
}