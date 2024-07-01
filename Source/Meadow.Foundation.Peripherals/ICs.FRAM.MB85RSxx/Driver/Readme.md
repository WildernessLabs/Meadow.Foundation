# Meadow.Foundation.ICs.FRAM.MB85RSxx

**MB85RSxx SPI FRAMs (MB85RS16 / MB85RS64V / MB85RS64T / MB85RS256TY / MB85RS1MT / MB85RS2MTA / MB85RS4MT / FM25V02 / MR45V064B)**

The **MB85RSxx** library is included in the **Meadow.Foundation.ICs.FRAM.MB85RSxx** nuget package and is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform.

This driver is part of the [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/) peripherals library, an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT applications.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Installation

You can install the library from within Visual studio using the the NuGet Package Manager or from the command line using the .NET CLI:

`dotnet add package Meadow.Foundation.ICs.FRAM.MB85RSxx`
## Usage

```csharp
MB85RSxx fram;

public override Task Initialize()
{
    Resolver.Log.Info("Initialize...");
    
    ISpiBus Spi5 = Device.CreateSpiBus(
                Device.Pins.SPI5_SCK,
                Device.Pins.SPI5_COPI,
                Device.Pins.SPI5_CIPO,
                new Frequency(48000, Frequency.UnitType.Kilohertz)

    // The memory size is automatically detected
    var csPort = Device.Pins.D02.CreateDigitalOutputPort();
    var wpPort = Device.Pins.D03.CreateDigitalOutputPort();
    var holdPort = Device.Pins.D04.CreateDigitalOutputPort();
    fram = new MB85RSxx(Spi5, csPort, wpPort, holdPort);

    return base.Initialize();
}

public override Task Run()
{
    Resolver.Log.Info("Write to fram");
    fram.Write(0, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });

    Resolver.Log.Info("Read from fram");
    byte[] memory = fram.Read(0, 16);

    for (ushort index = 0; index < 16; index++)
    {
        Thread.Sleep(50);
        Resolver.Log.Info("Byte: " + index + ", Value: " + memory[index]);
    }

    fram.Write(3, new byte[] { 10 });
    fram.Write(7, new byte[] { 1, 2, 3, 4 });
    memory = fram.Read(0, 16);

    for (ushort index = 0; index < 16; index++)
    {
        Thread.Sleep(50);
        Resolver.Log.Info("Byte: " + index + ", Value: " + memory[index]);
    }

    return base.Run();
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


