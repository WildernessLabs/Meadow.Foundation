# Meadow.Foundation.Motors.Stepper.GpioStepper

**Digital input stepper motor controller**

The **GpioStepper** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/).

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

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
