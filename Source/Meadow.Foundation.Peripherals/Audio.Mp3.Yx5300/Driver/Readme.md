# Meadow.Foundation.Audio.Mp3.Yx5300

**YX5300 serial MP3 player**

The **Yx5300** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/)

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/), to view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/)

## Usage

```
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
