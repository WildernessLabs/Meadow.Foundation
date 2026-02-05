# Meadow.Foundation.Displays.Ft800

**FT800/FT800CB Embedded Video Engine (EVE) display driver**

The FT800 is a GPU-based display controller from FTDI/Bridgetek that provides hardware-accelerated 2D graphics, built-in UI widgets, touch input, and audio capabilities.

## Features

- **IPixelDisplay compatible** - Works with MicroGraphics for standard drawing operations
- **GPU-accelerated graphics** - Hardware anti-aliased lines, circles, and shapes
- **Built-in widgets** - Buttons, sliders, gauges, clocks, progress bars, and more
- **ROM fonts** - 16 built-in fonts (no bitmap upload required)
- **Touch support** - ITouchScreen implementation with calibration
- **Tag-based touch detection** - Identify which object was touched

## Specifications

| Property | Value |
|----------|-------|
| Resolution | 480 x 272 (WQVGA) |
| Color Depth | 262K colors (18-bit RGB) |
| Interface | SPI (up to 30 MHz) |
| Touch | Resistive (built-in controller) |
| Graphics RAM | 256 KB |

## Purchase

* [Matrix Orbital EVE2 Series](https://www.matrixorbital.com/ftdi-eve)
* [Newhaven Display FT800CB](https://www.newhavendisplay.com/nhd43480272ftcsxv-p-6464.html)

## Usage

### Basic Setup with MicroGraphics

```csharp
var spiBus = Device.CreateSpiBus();

var display = new Ft800(
    spiBus: spiBus,
    chipSelectPin: Device.Pins.D02,
    powerDownPin: Device.Pins.D03,
    interruptPin: Device.Pins.D04  // Optional, for touch
);

// Use with MicroGraphics (compatibility mode)
var graphics = new MicroGraphics(display);

graphics.Clear(Color.Black);
graphics.DrawRectangle(10, 10, 100, 50, Color.Red, true);
graphics.DrawText(10, 70, "Hello World!", Color.White);
graphics.Show();
```

### GPU-Accelerated Graphics

```csharp
var gpu = new Ft800Graphics(display);

gpu.BeginDisplayList();
gpu.Clear(Color.DarkBlue);

// Hardware anti-aliased line
gpu.DrawLine(0, 0, 480, 272, Color.Yellow, 3);

// Rounded rectangle
gpu.DrawRoundedRectangle(50, 50, 200, 100, 15, Color.Red, true);

// Filled circle (large point)
gpu.DrawCircle(350, 136, 50, Color.Cyan, true);

gpu.EndDisplayList();
gpu.SwapDisplayList();
```

### Built-in Widgets

The FT800 co-processor provides hardware-rendered widgets:

```csharp
// Draw a 3D button
gpu.DrawButton(50, 50, 120, 40, Ft800Font.Size25, "Click Me");

// Draw an analog clock
gpu.DrawClock(240, 136, 80, 10, 30, 45);

// Draw a gauge
gpu.DrawGauge(400, 136, 60, 5, 4, 75, 100);

// Draw a progress bar
gpu.DrawProgress(50, 200, 380, 20, 65, 100);

// Draw a slider
gpu.DrawSlider(50, 240, 200, 15, 50, 100);
```

### Touch Input

```csharp
display.TouchDown += (sender, point) =>
{
    Console.WriteLine($"Touch at ({point.ScreenX}, {point.ScreenY})");

    // Check which object was touched (using tags)
    var tag = display.ReadTouchTag();
    if (tag == 1) { /* Button 1 pressed */ }
};

display.TouchUp += (sender, point) =>
{
    Console.WriteLine("Touch released");
};
```

### Using Touch Tags

Tags let you identify which object was touched:

```csharp
gpu.BeginDisplayList();
gpu.Clear(Color.Black);

// Set tag 1 for the first button
gpu.SetTag(1);
gpu.DrawButton(50, 50, 100, 40, Ft800Font.Size20, "Button 1", tag: 1);

// Set tag 2 for the second button
gpu.SetTag(2);
gpu.DrawButton(50, 100, 100, 40, Ft800Font.Size20, "Button 2", tag: 2);

gpu.EndDisplayList();
gpu.SwapDisplayList();

// Later, when touch occurs:
var tag = display.ReadTouchTag();
switch (tag)
{
    case 1: Console.WriteLine("Button 1 pressed"); break;
    case 2: Console.WriteLine("Button 2 pressed"); break;
}
```

## Wiring

| FT800 Pin | Meadow Pin | Description |
|-----------|------------|-------------|
| SCK | SCK | SPI Clock |
| MOSI | MOSI | SPI Data In |
| MISO | MISO | SPI Data Out |
| CS | D02 | Chip Select |
| PD | D03 | Power Down |
| INT | D04 | Interrupt (optional) |
| VCC | 3V3 | Power (3.3V) |
| GND | GND | Ground |

## ROM Fonts

The FT800 includes 16 built-in fonts:

| Font | Size | Handle |
|------|------|--------|
| Size8 | 8x8 | 16 |
| Size8x13 | 8x13 | 20 |
| Size8x16 | 8x16 | 18 |
| Size16 | 16x16 | 22 |
| Size16x24 | 16x24 | 24 |
| Size20 | 20x20 | 25 |
| Size20x32 | 20x32 | 26 |
| Size25 | 25x40 | 27 |
| Size28 | 28x48 | 28 |
| Size31 | 31x52 | 29 |
| Size34 | 34x56 | 30 |
| Size49 | 49x64 | 31 |

## Performance Comparison

| Operation | MicroGraphics | Ft800Graphics |
|-----------|---------------|---------------|
| Full screen clear | ~250ms | ~1ms |
| Draw 100 lines | ~50ms | ~5ms |
| Draw button | N/A | ~1ms |
| Draw gauge | N/A | ~1ms |

## Additional Resources

- [FT800 Programmer's Guide](https://brtchip.com/wp-content/uploads/Support/Documentation/Programming_Guides/ICs/EVE/FT800_Series_Programmer_Guide.pdf)
- [FT800 Datasheet](https://brtchip.com/wp-content/uploads/Support/Documentation/Datasheets/ICs/EVE/DS_FT800_Embedded_Video_Engine.pdf)
- [Bridgetek EVE Documentation](https://brtchip.com/eve/)
