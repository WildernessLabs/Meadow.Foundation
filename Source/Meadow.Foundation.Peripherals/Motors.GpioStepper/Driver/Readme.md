# Meadow.Foundation.Motors.Stepper.GpioStepper

**Digital input stepper motor controller**

The **GpioStepper** library is included in the **Meadow.Foundation.Motors.Stepper.GpioStepper** nuget package and is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform.

This driver is part of the [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/) peripherals library, an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT applications.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Installation

You can install the library from within Visual studio using the the NuGet Package Manager or from the command line using the .NET CLI:

`dotnet add package Meadow.Foundation.Motors.Stepper.GpioStepper`
## Usage

```csharp
private IStepperMotor stepper;

private bool UseStepDirConfiguration { get; set; } = true;

public override Task Initialize()
{
    if (UseStepDirConfiguration)
    {
        // use a drive configured for STEP/DIR GPIOs
        stepper = new StepDirStepper(
            Device.Pins.D15.CreateDigitalOutputPort(),
            Device.Pins.D14.CreateDigitalOutputPort(),
            stepsPerRevolution: 200);
    }
    else
    {
        // use a drive configured for CW/CCW GPIOs
        stepper = new CwCcwStepper(
            Device.Pins.D15.CreateDigitalOutputPort(),
            Device.Pins.D14.CreateDigitalOutputPort(),
            stepsPerRevolution: 200);
    }

    return base.Initialize();
}

public override Task Run()
{
    // return RunUntilCancelled();
    // return RunForSpecifiedTime();
    // return RunToSpecificPositions();
    return RotateSpecifiedAmount();
}

private async Task RunUntilCancelled()
{
    var direction = RotationDirection.Clockwise;
    var rate = new AngularVelocity(1, AngularVelocity.UnitType.RevolutionsPerSecond);

    while (true)
    {
        var tokenSource = new CancellationTokenSource();

        Resolver.Log.Info($"Start running...");
        var task = stepper.Run(direction, rate, tokenSource.Token);

        Resolver.Log.Info($"wait for 3 seconds...");
        await Task.Delay(3000);

        Resolver.Log.Info($"cancelling motion...");
        tokenSource.Cancel();

        Resolver.Log.Info($"wait for motion to stop");
        task.Wait();

        Resolver.Log.Info($"motion stopped");

        direction = direction switch
        {
            RotationDirection.CounterClockwise => RotationDirection.Clockwise,
            _ => RotationDirection.CounterClockwise
        };

        await Task.Delay(500);
    }
}

private async Task RunForSpecifiedTime()
{
    while (true)
    {
        var direction = RotationDirection.Clockwise;
        var rate = new AngularVelocity(1, AngularVelocity.UnitType.RevolutionsPerSecond);

        Resolver.Log.Info($"Run for 2 seconds...");
        await stepper.RunFor(TimeSpan.FromSeconds(2), direction, rate);

        direction = RotationDirection.CounterClockwise;
        rate = new AngularVelocity(2, AngularVelocity.UnitType.RevolutionsPerSecond);

        await stepper.RunFor(TimeSpan.FromSeconds(2), direction, rate);
    }
}

private async Task RotateSpecifiedAmount()
{
    while (true)
    {
        for (var turns = 0.5d; turns <= 5; turns += 0.5)
        {
            Resolver.Log.Info($"Moving {turns:0.0} revolutions");

            var direction = RotationDirection.Clockwise;
            var rate = new AngularVelocity(4, AngularVelocity.UnitType.RevolutionsPerSecond);

            await stepper.Rotate(new Angle(turns, Angle.UnitType.Revolutions), direction, rate);

            await Task.Delay(1000);
        }
    }
}

private async Task RunToSpecificPositions()
{
    RotationDirection direction;

    var rate = new AngularVelocity(2, AngularVelocity.UnitType.RevolutionsPerSecond);

    // turn in smaller and smaller degree increments
    var increments = new double[] { 180, 90, 60, 45, 30 };

    while (true)
    {
        direction = RotationDirection.Clockwise;

        Resolver.Log.Info($"{direction}");

        foreach (var increment in increments)
        {
            Resolver.Log.Info($"Moving in increments of {increment} degrees");

            await stepper.GoTo(new Angle(0), direction, rate);
            await Task.Delay(1000);

            var nextPosition = 0d;

            while (nextPosition < 360)
            {
                Resolver.Log.Info($"Moving to {nextPosition} degrees");

                nextPosition += increment;

                await stepper.GoTo(new Angle(nextPosition, Meadow.Units.Angle.UnitType.Degrees), direction, rate);
                await Task.Delay(1000);
            }
        }

        await Task.Delay(3000);

        direction = RotationDirection.CounterClockwise;

        Resolver.Log.Info($"{direction}");

        foreach (var increment in increments)
        {
            Resolver.Log.Info($"Moving in increments of {increment} degrees");

            var nextPosition = 360d;

            await stepper.GoTo(new Angle(0), direction, rate);
            await Task.Delay(1000);

            while (nextPosition > 0)
            {
                Resolver.Log.Info($"Moving to {nextPosition} degrees");

                nextPosition -= increment;

                await stepper.GoTo(new Angle(nextPosition, Meadow.Units.Angle.UnitType.Degrees), direction, rate);
                await Task.Delay(1000);
            }
        }

        await Task.Delay(3000);

        Resolver.Log.Info($"--- Cycle complete ---");
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


