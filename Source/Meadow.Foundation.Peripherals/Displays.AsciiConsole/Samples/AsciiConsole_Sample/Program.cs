using Meadow;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Foundation.Graphics.MicroLayout;

namespace AsciiConsole_Sample;

internal class Program
{
    private static void Main(string[] args)
    {
        DrawShapes();
    }

    private static async Task MovingBox()
    {
        var colors = typeof(Color)
            .GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public)
            .Where(f => f.FieldType == typeof(Color))
            .Select(c => (Color)c.GetValue(null)!)
            .ToArray();

        var colorIndex = 0;
        var display = new AsciiConsole(20, 15);
        var screen = new DisplayScreen(display);
        var box = new Box(0, 0, 4, 3)
        {
            ForeColor = colors[colorIndex]
        };
        screen.Controls.Add(box);

        var x = 0;
        var y = 0;
        var xdir = 1;
        var ydir = 1;

        while (true)
        {
            screen.BeginUpdate();
            box.Top = y;
            box.Left = x;
            box.ForeColor = colors[colorIndex];
            screen.EndUpdate();

            await Task.Delay(500);

            if ((x >= display.Width - 4) || (x < 0))
            {
                xdir *= -1;
            }
            if ((y >= display.Height - 4) || (y < 0))
            {
                ydir *= -1;
            }
            x += (1 * xdir);
            y += (1 * ydir);

            colorIndex++;
            if (colorIndex >= colors.Length) colorIndex = 0;
        }
    }

    private static void DrawShapes()
    {
        var display = new AsciiConsole(80, 60);

        var graphics = new MicroGraphics(display)
        {
            IgnoreOutOfBoundsPixels = true,
        };

        graphics.Clear();

        graphics.DrawTriangle(5, 5, 30, 30, 5, 30, Color.Red, false);
        graphics.DrawRectangle(10, 12, 40, 20, Color.Yellow, false);
        graphics.DrawCircle(20, 20, 20, Color.White, false);

        graphics.Show();
    }

    private static async Task CycleColors()
    {
        var display = new AsciiConsole(40, 30);

        var colors = typeof(Color).GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public).Where(c => c.FieldType == typeof(Color));
        foreach (var color in colors)
        {
            for (var x = 0; x < 16; x++)
            {
                for (var y = 0; y < 16; y++)
                {
                    var c = (Color)color.GetValue(null)!;

                    display.DrawPixel(x, y, c);
                }
            }

            display.Show();

            await Task.Delay(100);
        }
    }
}
