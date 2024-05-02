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
    }

    public static void Run()
    {
        Task.Run(() =>
        {
            int circleRadius = 40;
            int circleSpeed = 4;
            int x = circleRadius;
            int y = circleRadius;
            bool xDirection = true;
            bool yDirection = true;

            while (true)
            {
                graphics.Clear();

                graphics.DrawText(10, 10, "Silk.NET", Color.White);

                graphics.DrawText(10, 40, "1234567890!@#$%^&*(){}[],./<>?;':", Color.LawnGreen);
                graphics.DrawText(10, 70, "ABCDEFGHIJKLMNOPQRSTUVWXYZ", Color.Cyan);
                graphics.DrawText(10, 100, "abcdefghijklmnopqrstuvwxyz", Color.Yellow);
                graphics.DrawText(10, 130, "Temp: 21.5°C", Color.Orange);

                graphics.DrawTriangle(10, 220, 50, 260, 10, 260, Color.Red);
                graphics.DrawRectangle(20, 185, 80, 40, Color.Yellow, false);

                graphics.DrawCircle(x, y, circleRadius, Color.Blue, false);
                graphics.Show();

                x += xDirection ? circleSpeed : -circleSpeed;
                y += yDirection ? circleSpeed : -circleSpeed;

                if (x > display!.Width - circleRadius) { xDirection = false; }
                else if (x < circleRadius) { xDirection = true; }

                if (y > display!.Height - circleRadius) { yDirection = false; }
                else if (y < circleRadius) { yDirection = true; }
            }
        });

        display!.Run();
    }
}

//<!=SNOP=>