using Meadow;
using Meadow.Foundation;
using Meadow.Foundation.Displays;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Hardware;
using MicroLayout;

public class MeadowApp : App<Windows>
{
    private Ft232h _expander = new Ft232h();
    private DisplayScreen _screen;

    public static async Task Main(string[] args)
    {
        await MeadowOS.Start(args);
    }

    public override Task Initialize()
    {
        var display = new Max7219(
            _expander.CreateSpiBus(Max7219.DefaultSpiBusSpeed),
            _expander.Pins.C0.CreateDigitalOutputPort(), // CS
            deviceRows: 4,
            deviceColumns: 1);


        _screen = new DisplayScreen(display, Meadow.Foundation.Graphics.RotationType._270Degrees);
        _screen.BackgroundColor = Color.Black;

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
