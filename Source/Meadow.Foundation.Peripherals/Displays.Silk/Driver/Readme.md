# Meadow.Foundation.Displays.Silk

**Display driver for Meadow using Silk.NET and SkiaSharp**

The **Meadow.Silk** library is included in the **Meadow.Foundation.Displays.Silk** nuget package and is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform.

This driver is part of the [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/) peripherals library, an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT applications.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Installation

You can install the library from within Visual studio using the the NuGet Package Manager or from the command line using the .NET CLI:

`dotnet add package Meadow.Foundation.Displays.Silk`
## Usage

```csharp
public class Program
{
static SilkDisplay? display;
static MicroGraphics graphics = default!;

static PixelBufferBase image = default!;

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

    image = LoadJpeg() as PixelBufferBase;
}

public static void Run()
{
    Task.Run(() =>
    {
        var grayImage = image.Convert<BufferGray8>();

        var scaledImage = image.Resize<BufferGray8>(320, 320);

        var rotatedImage = image.Rotate<BufferGray8>(new Meadow.Units.Angle(60));

        graphics.Clear();

        //draw the image centered
        graphics.DrawBuffer((display!.Width - rotatedImage.Width) / 2,
            (display!.Height - rotatedImage.Height) / 2, rotatedImage);

        graphics.Show();
    });

    display!.Run();
}

static IPixelBuffer LoadJpeg()
{
    var jpgData = LoadResource("maple.jpg");

    var decoder = new JpegDecoder();
    var jpg = decoder.DecodeJpeg(jpgData);

    Console.WriteLine($"Jpeg decoded is {jpg.Length} bytes, W: {decoder.Width}, H: {decoder.Height}");

    return new BufferRgb888(decoder.Width, decoder.Height, jpg);
}

static byte[] LoadResource(string filename)
{
    var assembly = Assembly.GetExecutingAssembly();
    var resourceName = $"Silk_Image_Sample.{filename}";

    using Stream stream = assembly.GetManifestResourceStream(resourceName);
    using var ms = new MemoryStream();
    stream.CopyTo(ms);
    return ms.ToArray();
}
}
```
## How to Contribute

- **Found a bug?** [Report an issue](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Have a **feature idea or driver request?** [Open a new feature request](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Want to **contribute code?** Fork the [Meadow.Foundation](https://github.com/WildernessLabs/Meadow.Foundation) repository and submit a pull request against the `develop` branch


## Need Help?

If you have questions or need assistance, please join the Wilderness Labs [community on Slack](http://slackinvite.wildernesslabs.co/).
## About Meadow

Meadow is a complete, IoT platform with defense-grade security that runs full .NET applications on embeddable microcontrollers and Linux single-board computers including Raspberry Pi and NVIDIA Jetson.

### Build

Use the full .NET platform and tooling such as Visual Studio and plug-and-play hardware drivers to painlessly build IoT solutions.

### Connect

Utilize native support for WiFi, Ethernet, and Cellular connectivity to send sensor data to the Cloud and remotely control your peripherals.

### Deploy

Instantly deploy and manage your fleet in the cloud for OtA, health-monitoring, logs, command + control, and enterprise backend integrations.


