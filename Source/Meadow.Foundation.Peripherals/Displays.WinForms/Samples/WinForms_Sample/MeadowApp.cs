using Meadow;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using System.Windows.Forms;

namespace WinForms_Sample;

//<!=SNIP=>

public class MeadowApp : App<Meadow.Windows>
{
    WinFormsDisplay? display;
    MicroGraphics graphics = default!;

    public override Task Initialize()
    {
        display = new WinFormsDisplay(320, 240, displayScale: 1.5f);

        graphics = new MicroGraphics(display)
        {
            CurrentFont = new Font12x20(),
            Stroke = 1
        };

        _ = Task.Run(() =>
        {
            graphics.Clear();

            graphics.DrawTriangle(10, 30, 50, 50, 10, 50, Color.Red);
            graphics.DrawRectangle(20, 45, 40, 20, Color.Yellow, false);
            graphics.DrawCircle(50, 50, 40, Color.Blue, false);
            graphics.DrawText(5, 5, "Meadow on WinForms", Color.White);

            graphics.Show();
        });

        return Task.CompletedTask;
    }

    public override Task Run()
    {
        Application.Run(display!);

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