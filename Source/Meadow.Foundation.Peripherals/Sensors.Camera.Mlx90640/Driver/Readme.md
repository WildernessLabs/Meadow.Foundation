# Meadow.Foundation.Sensors.Camera.Mlx90640

**Mlx90640 I2C far infrared thermal sensor array camera**

The **Mlx90640** library is included in the **Meadow.Foundation.Sensors.Camera.Mlx90640** nuget package and is designed for the [Wilderness Labs](https://www.wildernesslabs.co) Meadow .NET IoT platform.

This driver is part of the [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/) peripherals library, an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT applications.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Installation

You can install the library from within Visual Studio using the NuGet Package Manager or from the command line using the .NET CLI:

`dotnet add package Meadow.Foundation.Sensors.Camera.Mlx90640`
## Usage

```csharp
Mlx90640 sensor;

public override Task Initialize()
{
    Resolver.Log.Info("Initialize...");

    var i2cBus = Device.CreateI2cBus(I2cBusSpeed.Fast);
    sensor = new Mlx90640(i2cBus);

    return Task.CompletedTask;
}

public override Task Run()
{
    bool showTempArrayAsAsciiArt = false;

    Resolver.Log.Info("Run sample...");

    float[] frame;

    Resolver.Log.Info($"Serial #:{sensor.SerialNumber}");

    sensor.SetMode(Mlx90640.Mode.Chess);
    Resolver.Log.Info($"Current Mode: {sensor.GetMode()}");

    sensor.SetResolution(Mlx90640.Resolution.EighteenBit);
    Resolver.Log.Info($"Current resolution: {sensor.GetResolution()}");

    sensor.SetRefreshRate(Mlx90640.RefreshRate._2hz);
    Resolver.Log.Info($"Current frame rate: {sensor.GetRefreshRate()}");

    Resolver.Log.Info($"Broken Pixels: {sensor.Config.BrokenPixels.Count}");
    Resolver.Log.Info($"Outlier Pixels: {sensor.Config.OutlierPixels.Count}");
    Resolver.Log.Info($"Broken Pixels has adjacent broken pixel: {sensor.Config.BrokenPixelHasAdjacentBrokenPixel}");
    Resolver.Log.Info($"Broken Pixels has adjacent Outlier pixel: {sensor.Config.BrokenPixelHasAdjacentOutlierPixel}");
    Resolver.Log.Info($"Outlier Pixels has adjacent Outlier pixel: {sensor.Config.OutlierPixelHasAdjacentOutlierPixel}");

    Thread.Sleep(2000);

    while (true)
    {
        Thread.Sleep(1000);

        frame = sensor.ReadRawData();

        Resolver.Log.Info("");

        //Print out each value
        for (byte h = 0; h < 24; h++)
        {
            StringBuilder logLine = new StringBuilder();
            for (byte w = 0; w < 32; w++)
            {
                float t = frame[h * 32 + w];
                //View sensor data as ASCII art. It is easier to see shapes, like your fingers.
                if (!showTempArrayAsAsciiArt)
                {
                    //Write the Temp value
                    logLine.Append($"{t:0},");
                }
                else
                {
                    //Write the ASCII art character (thresholds in Celsius)
                    char c = '&';
                    if (t < 20) c = ' ';
                    else if (t < 23) c = '.';
                    else if (t < 25) c = '-';
                    else if (t < 27) c = '*';
                    else if (t < 29) c = '+';
                    else if (t < 31) c = 'x';
                    else if (t < 33) c = '%';
                    else if (t < 35) c = '#';
                    else if (t < 37) c = '$';
                    logLine.Append(c);
                }
            }

            Resolver.Log.Info(logLine.ToString());
        }
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

Meadow is a complete IoT platform with defense-grade security that runs full .NET applications on embeddable microcontrollers and Linux single-board computers including Raspberry Pi and NVIDIA Jetson.

### Build

Use the full .NET platform and tooling such as Visual Studio and plug-and-play hardware drivers to painlessly build IoT solutions.

### Connect

Utilize native support for WiFi, Ethernet, and Cellular connectivity to send sensor data to the Cloud and remotely control your peripherals.

### Deploy

Instantly deploy and manage your fleet in the cloud for OTA, health-monitoring, logs, command + control, and enterprise backend integrations.


