using Meadow;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics.MicroLayout;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Foundation.Sensors.Camera;
using System.Windows.Forms;

public class MeadowApp : App<Windows>
{
    private Ft232h _ft232h;
    private WinFormsDisplay _display;
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

        _display = new WinFormsDisplay(320, 480);

        _ft232h = new Ft232h(false);
        var bus = _ft232h.CreateI2cBus();
        _camera = new Amg8833(bus);

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

    public override Task Run()
    {
        _ = Task.Run(async () =>
        {
            await Task.Delay(1000);

            while (true)
            {
                var pixels = _camera.ReadPixels();

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
        });

        Application.Run(_display);

        return Task.CompletedTask;
    }
}