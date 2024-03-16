using Meadow;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.MicroLayout;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Peripherals.Displays;

public class MeadowApp : App<Windows>
{
    private DisplayScreen? screen;

    public override Task Initialize()
    {
        var expander = FtdiExpanderCollection.Devices[0];

        var display = new Ili9341
        (
            spiBus: expander.CreateSpiBus(),
            chipSelectPin: expander.Pins.C0,
            dcPin: expander.Pins.C2,
            resetPin: expander.Pins.C1,
            width: 240, height: 320,
            colorMode: ColorMode.Format16bppRgb565
        );

        screen = new DisplayScreen(display, RotationType._270Degrees)
        {
            BackgroundColor = Color.Black
        };

        return base.Initialize();
    }

    public override Task Run()
    {
        Text();

        return base.Run();
    }

    public void Text()
    {
        var label = new Label(0, 0, screen!.Width, screen.Height);
        label.Font = new Font12x20();
        label.HorizontalAlignment = HorizontalAlignment.Center;
        label.VerticalAlignment = VerticalAlignment.Center;
        label.TextColor = Color.Red;
        label.Text = "HELLO";

        screen.Controls.Add(label);

    }

    public void TextOnBox()
    {
        var box = new Box(0, 0, screen!.Width / 4, screen.Height);
        box.ForeColor = Color.Red;
        var label = new Label(0, 0, screen.Width / 4, screen.Height);
        label.HorizontalAlignment = HorizontalAlignment.Center;
        label.VerticalAlignment = VerticalAlignment.Center;
        label.TextColor = Color.Black;
        label.Text = "Meadow";

        screen.Controls.Add(box, label);

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
        var box = new Box(0, 0, screen!.Width / 4, screen.Height);
        box.ForeColor = Color.Red;

        screen.Controls.Add(box);

        var direction = 1;
        var speed = 1;

        while (true)
        {
            var left = box.Left + (speed * direction);

            box.Left = left;

            if ((box.Right >= screen.Width) || box.Left <= 0)
            {
                direction *= -1;
            }

            Thread.Sleep(50);
        }

    }

    public static async Task Main(string[] args)
    {
        await MeadowOS.Start(args);
    }
}