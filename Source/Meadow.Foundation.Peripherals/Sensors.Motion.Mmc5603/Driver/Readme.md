# Meadow.Foundation.Sensors.Motion.Mmc5603

**MMC5603 I2C 3 axis digital magnetometer**

The **MMC5603** library is included in the **Meadow.Foundation.Sensors.Motion.Mmc5603** nuget package and is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform.

This driver is part of the [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/) peripherals library, an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT applications.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Installation

You can install the library from within Visual studio using the the NuGet Package Manager or from the command line using the .NET CLI:

`dotnet add package Meadow.Foundation.Sensors.Motion.Mmc5603`
## Usage

```csharp
private Mmc5603 sensor;

public override Task Initialize()
{
    Resolver.Log.Info("Initialize...");

    sensor = new Mmc5603(Device.CreateI2cBus());

    // classical .NET events can  be used
    sensor.Updated += (sender, result) =>
    {
        Resolver.Log.Info($"Magnetic Field: [X:{result.New.X.MicroTesla:N2}," +
            $"Y:{result.New.Y.MicroTesla:N2}," +
            $"Z:{result.New.Z.MicroTesla:N2} (MicroTeslas)]");
    };

    // Example that uses an IObservable subscription to only be notified when the filter is satisfied
    var consumer = Mmc5603.CreateObserver(
        handler: result => Resolver.Log.Info($"Observer: [x] changed by threshold; new [x]: X:{result.New.X.MicroTesla:N2}, old: X:{result.Old?.X.MicroTesla:N2}"),
        // only notify if there's a greater than 1 micro tesla on the Y axis
        filter: result =>
        {
            if (result.Old is { } old)
            {
                return (result.New - old).Y > new MagneticField(1, MU.MicroTesla);
            }
            return false;
        });

    sensor.Subscribe(consumer);

    return Task.CompletedTask;
}

public override async Task Run()
{
    //Read from sensor
    var result = await sensor.Read();

    //output initial readings text to console
    Resolver.Log.Info("Initial Readings:");
    Resolver.Log.Info(
        $"Magnetic field: [X:{result.X.MicroTesla:N2}," +
        $"Y:{result.Y.MicroTesla:N2}," +
        $"Z:{result.Z.MicroTesla:N2} (MicroTeslas)]");

    sensor.StartUpdating(TimeSpan.FromMilliseconds(1500));
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


