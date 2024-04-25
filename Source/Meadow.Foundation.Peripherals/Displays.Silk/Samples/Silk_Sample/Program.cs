using Meadow;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;

namespace SilkDisplay_Sample;

//<!=SNIP=>

public class Program 
{
    static SilkDisplay? display;
    static MicroGraphics graphics = default!;

    public static void Main()
    {
        Initialize();

        Run();

        Thread.Sleep(Timeout.Infinite);
    }

    public static void Initialize()
    {
        display = new SilkDisplay(640, 480, displayScale: 1f);
        

        graphics = new MicroGraphics(display)
        {
            CurrentFont = new Font16x24(),
            Stroke = 1
        };

        graphics.Clear(Color.Purple, false);
    }

    public static void Run()
    {
        for(int i = 0; i < 100; i++)
        {
            graphics.Clear();

            graphics.DrawText(10, 10, "Silk.NET", Color.White);

            graphics.DrawText(10, 40, "1234567890!@#$%^&*(){}[],./<>?;':", Color.LawnGreen);
            graphics.DrawText(10, 70, "ABCDEFGHIJKLMNOPQRSTUVWXYZ", Color.Cyan);
            graphics.DrawText(10, 100, "abcdefghijklmnopqrstuvwxyz", Color.Yellow);
            graphics.DrawText(10, 130, "Temp: 21.5°C", Color.Orange);

            graphics.DrawTriangle(10, 220, 50, 260, 10, 260, Color.Red);
            graphics.DrawRectangle(20 + i, 185 + i, 80, 40, Color.Yellow, false);
            graphics.DrawCircle(50 + i, 240 + i, 40, Color.Blue, false);

            graphics.Show();
        }
    }
}

//<!=SNOP=>