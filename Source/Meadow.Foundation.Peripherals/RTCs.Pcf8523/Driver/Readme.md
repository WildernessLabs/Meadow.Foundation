# Meadow.Foundation.RTCs.Pcf8523

**Pcf8523 I2C real time clock**

The **Pcf8523** library is included in the **Meadow.Foundation.RTCs.Pcf8523** nuget package and is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform.

This driver is part of the [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/) peripherals library, an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT applications.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Installation

You can install the library from within Visual studio using the the NuGet Package Manager or from the command line using the .NET CLI:

`dotnet add package Meadow.Foundation.RTCs.Pcf8523`
## Usage

```csharp
private Pcf8523 rtc;

public override Task Initialize()
{
    Resolver.Log.Info("Initializing...");

    rtc = new Pcf8523(Device.CreateI2cBus());

    return base.Initialize();
}

public override async Task Run()
{
    DateTimeOffset dateTime;
    var running = rtc.IsRunning;

    Resolver.Log.Info($"{(running ? "is running" : "is not running")}");

    if (!running)
    {
        Resolver.Log.Info(" Starting RTC...");
        rtc.IsRunning = true;
    }

    dateTime = rtc.GetTime();
    Resolver.Log.Info($" RTC current time is: {dateTime:MM/dd/yy HH:mm:ss}");

    dateTime = new DateTime(2030, 2, 15);
    Resolver.Log.Info($" Setting RTC to : {dateTime:MM/dd/yy HH:mm:ss}");
    rtc.SetTime(dateTime);

    dateTime = rtc.GetTime();
    Resolver.Log.Info($" RTC current time is: {dateTime:MM/dd/yy HH:mm:ss}");

    // Test Timer A
    Resolver.Log.Info("Setting Timer A for 5 seconds...");
    rtc.SetTimerA(5, DelayTimeUnit.Seconds);

    // Test Timer B
    Resolver.Log.Info("Setting Timer B for 2 seconds...");
    rtc.SetTimerB(2, DelayTimeUnit.Seconds);

    await Task.Delay(2000);

    if (rtc.HasTimerAInterruptTriggered)
    {
        Resolver.Log.Info("Timer A SUCCESS");
    }
    else
    {
        Resolver.Log.Info("Timer A FAILED");
    }

    if (rtc.HasTimerBInterruptTriggered)
    {
        Resolver.Log.Info("Timer B SUCCESS");
    }
    else
    {
        Resolver.Log.Info("Timer B FAILED");
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


