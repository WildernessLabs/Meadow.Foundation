# Meadow.Foundation.Audio.Mp3.Yx5300

**YX5300 serial MP3 player**

The **Yx5300** library is included in the **Meadow.Foundation.Audio.Mp3.Yx5300** nuget package and is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform.

This driver is part of the [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/) peripherals library, an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT applications.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Installation

You can install the library from within Visual studio using the the NuGet Package Manager or from the command line using the .NET CLI:

`dotnet add package Meadow.Foundation.Audio.Mp3.Yx5300`
## Usage

```csharp
Yx5300 mp3Player;

public override Task Initialize()
{
    Resolver.Log.Info("Initialize...");

    mp3Player = new Yx5300(Device, Device.PlatformOS.GetSerialPortName("COM4"));

    return Task.CompletedTask;
}

public override async Task Run()
{
    mp3Player.SetVolume(15);

    var status = await mp3Player.GetStatus();
    Resolver.Log.Info($"Status: {status}");

    var count = await mp3Player.GetNumberOfTracksInFolder(0);
    Resolver.Log.Info($"Number of tracks: {count}");

    mp3Player.Play();

    await Task.Delay(5000); //leave playing for 5 seconds

    mp3Player.Next();

    await Task.Delay(5000); //leave playing for 5 seconds
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


