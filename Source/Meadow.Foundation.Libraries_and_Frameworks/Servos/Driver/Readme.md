# Meadow.Foundation.Servos

**PWM generic servo controller**

The **Servos** library is included in the **Meadow.Foundation.Servos** nuget package and is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform.

This driver is part of the [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/) peripherals library, an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT applications.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Installation

You can install the library from within Visual studio using the the NuGet Package Manager or from the command line using the .NET CLI:

`dotnet add package Meadow.Foundation.Servos`
## Usage

```csharp
private IAngularServo servo;
private IPwmPort pwm;

public override async Task Initialize()
{
    Resolver.Log.Info("Initialize...");

    pwm = Device.Pins.D08.CreatePwmPort(50.Hertz());

    await RangeFinder();
    servo = new Mg90s(pwm);

    //            return Task.CompletedTask;
}

private async Task RangeFinder()
{
    var min = 200;
    var max = 3000;
    var p = min;
    var step = 10;
    var direction = 1;

    pwm.Start();

    while (true)
    {
        Resolver.Log.Info($"Duration: {p} us");

        pwm.Duration = TimePeriod.FromMicroseconds(p);
        await Task.Delay(200);

        var test = p + (step * direction);
        if (test < min || test > max) direction *= -1;
        p = p + (step * direction);
    }
}

public override async Task Run()
{
    var center = 0;
    var step = 10;
    var direction = 1;
    var target = center;

    await Task.Delay(1000);

    while (true)
    {
        var test = target + (step * direction);
        if (test > servo.MaximumAngle.Degrees || test < servo.MinimumAngle.Degrees)
        {
            direction *= -1;
            test = target + (step * direction);
        }
        target = test;
        var delay = target * 10;
        if (delay < 10) delay = 10;

        Resolver.Log.Info($"Rotating to {target}");
        servo.RotateTo(new Angle(target, Angle.UnitType.Degrees));
        await Task.Delay(delay);
        Resolver.Log.Info($"Rotating to {-target}");
        servo.RotateTo(new Angle(-target, Angle.UnitType.Degrees));
        await Task.Delay(delay);
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


