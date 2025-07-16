# Meadow.Foundation.RTCs.Ab0805

**Ab0805 I2C real time clock**

The **Ab0805** library is included in the **Meadow.Foundation.RTCs.Ab0805** nuget package and is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform.

This driver is part of the [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/) peripherals library, an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT applications.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Installation

You can install the library from within Visual studio using the the NuGet Package Manager or from the command line using the .NET CLI:

`dotnet add package Meadow.Foundation.RTCs.Ab0805`
## Usage

```csharp
private Ab0805 rtc;

public override Task Initialize()
{
    Resolver.Log.Info("Initializing...");

    rtc = new Ab0805(Device.CreateI2cBus());

    return base.Initialize();
}

public override async Task Run()
{
    // Test basic RTC functionality
    await TestBasicRTC();

    // Test countdown timers
    await TestCountdownTimers();
}

private async Task TestBasicRTC()
{
    Resolver.Log.Info("=== Testing Basic RTC Functionality ===");

    var running = rtc.IsRunning;
    Resolver.Log.Info($"RTC {(running ? "is running" : "is not running")}");

    if (!running)
    {
        Resolver.Log.Info("Starting RTC...");
        rtc.IsRunning = true;
    }

    var currentTime = rtc.GetTime();
    Resolver.Log.Info($"RTC current time: {currentTime:MM/dd/yy HH:mm:ss}");

    // Set RTC to a known time for testing
    var testTime = new DateTime(2025, 6, 15, 14, 30, 0);
    Resolver.Log.Info($"Setting RTC to: {testTime:MM/dd/yy HH:mm:ss}");
    rtc.SetTime(testTime);

    currentTime = rtc.GetTime();
    Resolver.Log.Info($"RTC time after setting: {currentTime:MM/dd/yy HH:mm:ss}");

    await Task.Delay(2000);

    currentTime = rtc.GetTime();
    Resolver.Log.Info($"RTC time after 2 second delay: {currentTime:MM/dd/yy HH:mm:ss}");
}

private async Task TestCountdownTimers()
{
    Resolver.Log.Info("\n=== Testing Countdown Timer Functionality ===");

    await TestAlarm();
    await Task.Delay(1000);
    await TestBasicTimer();
}

private async Task TestBasicTimer()
{
    Resolver.Log.Info("\n--- Test 1: Basic 2-second countdown timer ---");

    rtc.ResetTimer();

    Resolver.Log.Info("Starting 2-second countdown timer...");
    rtc.StartTimer(5, Ab0805.DelayTimeUnit.Seconds);

    var startTime = DateTime.Now;
    TimeSpan elapsed;

    while (rtc.HasTimerEnded == false)
    {
        elapsed = DateTime.Now - startTime;
        Resolver.Log.Info($"Elapsed: {elapsed.TotalSeconds:F1}s");

        await Task.Delay(1000);
    }

    elapsed = DateTime.Now - startTime;
    Resolver.Log.Info($"✓ Timer completed! Interrupt fired after {elapsed.TotalSeconds:F1}s");
    rtc.ResetTimer();
}

private async Task TestAlarm()
{
    Resolver.Log.Info("\n--- Test 2: Alarm 5 seconds in the future ---");

    DateTimeOffset alarmTime = rtc.GetTime().AddSeconds(5);

    Resolver.Log.Info("Monitoring alarm...");
    rtc.SetAlarm(alarmTime);

    var startTime = DateTime.Now;
    TimeSpan elapsed;

    while (rtc.HasAlarmTriggered == false)
    {
        elapsed = DateTime.Now - startTime;
        Resolver.Log.Info($"Elapsed: {elapsed.TotalSeconds:F1}s");

        await Task.Delay(1000);
    }

    elapsed = DateTime.Now - startTime;
    Resolver.Log.Info($"✓ Alarm triggered! Interrupt fired after {elapsed.TotalSeconds:F1}s");

    await Task.Delay(5000);
    rtc.ResetAlarm();
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


