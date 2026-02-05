using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Displays;
using Meadow.Foundation.Graphics;
using Meadow.Hardware;
using System;
using System.Threading.Tasks;

namespace Ft800_Sample;

public class MeadowApp : App<F7FeatherV2>
{
    private Ft800? display;
    private Ft800Graphics? gpu;
    private MicroGraphics? graphics;

    public override Task Initialize()
    {
        Resolver.Log.Info("Initializing FT800 display...");

        try
        {
            // Create the SPI bus
            var spiBus = Device.CreateSpiBus();

            // Create the FT800 display driver
            display = new Ft800(
                spiBus: spiBus,
                chipSelectPin: Device.Pins.D02,
                powerDownPin: Device.Pins.D03,
                interruptPin: Device.Pins.D04
            );

            Resolver.Log.Info("FT800 initialized successfully!");

            // Create GPU-accelerated graphics
            gpu = new Ft800Graphics(display);

            // Create MicroGraphics for compatibility mode demo
            graphics = new MicroGraphics(display);

            // Set up touch events
            display.TouchDown += OnTouchDown;
            display.TouchUp += OnTouchUp;
            display.TouchMoved += OnTouchMoved;

            Resolver.Log.Info("Ready!");
        }
        catch (Exception ex)
        {
            Resolver.Log.Error($"Error initializing FT800: {ex.Message}");
        }

        return Task.CompletedTask;
    }

    public override async Task Run()
    {
        if (display == null || gpu == null || graphics == null)
        {
            Resolver.Log.Error("Display not initialized");
            return;
        }

        // Demo 1: MicroGraphics compatibility mode
        Resolver.Log.Info("Demo 1: MicroGraphics mode");
        DemoMicroGraphics();
        await Task.Delay(3000);

        // Demo 2: GPU-accelerated primitives
        Resolver.Log.Info("Demo 2: GPU primitives");
        DemoGpuPrimitives();
        await Task.Delay(3000);

        // Demo 3: Built-in widgets
        Resolver.Log.Info("Demo 3: Widgets");
        DemoWidgets();
        await Task.Delay(3000);

        // Demo 4: Interactive dashboard
        Resolver.Log.Info("Demo 4: Dashboard");
        await DemoDashboard();
    }

    /// <summary>
    /// Demo using standard MicroGraphics (compatibility mode)
    /// </summary>
    private void DemoMicroGraphics()
    {
        graphics!.Clear(Color.DarkBlue);

        // Draw some shapes
        graphics.DrawRectangle(10, 10, 100, 50, Color.Red, true);
        graphics.DrawRectangle(120, 10, 100, 50, Color.Green, true);
        graphics.DrawRectangle(230, 10, 100, 50, Color.Blue, true);

        // Draw circles
        graphics.DrawCircle(60, 150, 40, Color.Yellow, true);
        graphics.DrawCircle(160, 150, 40, Color.Cyan, false);
        graphics.DrawCircle(260, 150, 40, Color.Magenta, true);

        // Draw text
        graphics.CurrentFont = new Font12x20();
        graphics.DrawText(10, 220, "MicroGraphics Mode", Color.White);
        graphics.DrawText(10, 245, "Standard IPixelDisplay", Color.Gray);

        graphics.Show();
    }

    /// <summary>
    /// Demo using GPU-accelerated primitives
    /// </summary>
    private void DemoGpuPrimitives()
    {
        gpu!.BeginDisplayList();
        gpu.Clear(Color.Black);

        // Draw anti-aliased lines
        gpu.SetLineWidth(2);
        gpu.DrawLine(0, 0, 480, 272, Color.Red, 2);
        gpu.DrawLine(480, 0, 0, 272, Color.Green, 2);

        // Draw filled rectangles
        gpu.DrawRectangle(50, 50, 100, 60, Color.Purple, true);
        gpu.DrawRoundedRectangle(200, 50, 120, 60, 10, Color.Orange, true);

        // Draw circles (filled = large points)
        gpu.DrawCircle(400, 100, 40, Color.Cyan, true);
        gpu.DrawCircle(400, 100, 50, Color.White, false);

        // Draw a line strip (polygon outline)
        gpu.DrawLineStrip(new[]
        {
            (50, 180), (100, 140), (150, 180), (100, 220), (50, 180)
        }, Color.Yellow, 2);

        gpu.EndDisplayList();
        gpu.SwapDisplayList();
    }

    /// <summary>
    /// Demo using built-in widgets
    /// </summary>
    private void DemoWidgets()
    {
        // Draw a clock
        gpu!.DrawClock(100, 136, 80, DateTime.Now.Hour, DateTime.Now.Minute, DateTime.Now.Second);
    }

    /// <summary>
    /// Demo interactive dashboard with updating values
    /// </summary>
    private async Task DemoDashboard()
    {
        int gaugeValue = 0;
        int direction = 1;

        for (int i = 0; i < 100; i++)
        {
            // Update gauge value
            gaugeValue += direction * 2;
            if (gaugeValue >= 100 || gaugeValue <= 0)
            {
                direction = -direction;
            }

            // Draw gauge
            gpu!.DrawGauge(120, 136, 80, 5, 4, (ushort)gaugeValue, 100);

            await Task.Delay(50);
        }
    }

    private void OnTouchDown(ITouchScreen sender, TouchPoint point)
    {
        Resolver.Log.Info($"Touch DOWN at ({point.ScreenX}, {point.ScreenY})");
    }

    private void OnTouchUp(ITouchScreen sender, TouchPoint point)
    {
        Resolver.Log.Info($"Touch UP at ({point.ScreenX}, {point.ScreenY})");

        // Check which widget was touched
        var tag = display!.ReadTouchTag();
        if (tag > 0)
        {
            Resolver.Log.Info($"  Tag: {tag}");
        }
    }

    private void OnTouchMoved(ITouchScreen sender, TouchPoint point)
    {
        Resolver.Log.Trace($"Touch MOVE at ({point.ScreenX}, {point.ScreenY})");
    }
}
