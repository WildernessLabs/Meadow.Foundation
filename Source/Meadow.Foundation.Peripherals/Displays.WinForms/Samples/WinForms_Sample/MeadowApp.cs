using Meadow;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using System.Windows.Forms;

namespace WinForms_Sample;

//<!=SNIP=>

public class MeadowApp : App<Meadow.Windows>
{
    WinFormsDisplay? _display;
    MicroGraphics _graphics = default!;

    public override Task Initialize()
    {
        _display = new WinFormsDisplay(240, 320);

        _graphics = new MicroGraphics(_display)
        {
            CurrentFont = new Font12x20(),
            Stroke = 1
        };

        _ = Task.Run(() =>
        {
            Thread.Sleep(2000);

            _graphics.Clear();

            _graphics.DrawTriangle(10, 30, 50, 50, 10, 50, Color.Red);
            _graphics.DrawRectangle(20, 45, 40, 20, Color.Yellow, false);
            _graphics.DrawCircle(50, 50, 40, Color.Blue, false);
            _graphics.DrawText(5, 5, "Meadow F7", Color.White);

            _graphics.Show();
        });

        return Task.CompletedTask;
    }

    public override Task Run()
    {
        Application.Run(_display!);

        return Task.CompletedTask;
    }

    public static async Task Main(string[] args)
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        ApplicationConfiguration.Initialize();

        await MeadowOS.Start(args);
    }
}

//<!=SNOP=>