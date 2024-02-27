# Meadow.Foundation.Sensors.Hid.Mpr121

**Freescale Semiconductor MPR121 I2C capacitive keypad controller**

The **Mpr121** library is included in the **Meadow.Foundation.Sensors.Hid.Mpr121** nuget package and is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform.

This driver is part of the [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/) peripherals library, an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT applications.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Installation

You can install the library from within Visual studio using the the NuGet Package Manager or from the command line using the .NET CLI:

`dotnet add package Meadow.Foundation.Sensors.Hid.Mpr121`
## Usage

```csharp
public override Task Initialize()
{
    Resolver.Log.Info("Initialize...");

    var sensor = new Mpr121(Device.CreateI2cBus(Meadow.Hardware.I2cBusSpeed.Standard), 90, 100);
    sensor.ChannelStatusesChanged += Sensor_ChannelStatusesChanged;

    return Task.CompletedTask;
}

private void Sensor_ChannelStatusesChanged(object sender, ChannelStatusChangedEventArgs e)
{
    string pads = string.Empty;

    for (int i = 0; i < e.ChannelStatus.Count; i++)
    {
        if (e.ChannelStatus[(Mpr121.Channels)i] == true)
        {
            pads += i + ", ";
        }
    }

    var msg = string.IsNullOrEmpty(pads) ? "none" : (pads + "touched");
    Resolver.Log.Info(msg);
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


