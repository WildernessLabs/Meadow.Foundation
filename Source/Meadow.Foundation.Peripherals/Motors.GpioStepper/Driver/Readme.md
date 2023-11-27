# Meadow.Foundation.Motors.Stepper.Em542s

**The Leadshine EM542S Stepper Motor Drive**

The **Em542s** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/).

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Usage

```csharp
private IStepperMotor stepper;

public override Task Initialize()
{
    stepper = new Em542s(
        Device.Pins.D15.CreateDigitalOutputPort(),
        Device.Pins.D14.CreateDigitalOutputPort(),
        inverseLogic: true);

    return base.Initialize();
}

public override Task Run()
{
    var direction = RotationDirection.Clockwise;

    // max rate for this drive
    var rate = new Meadow.Units.Frequency(200, Meadow.Units.Frequency.UnitType.Kilohertz);

    while (true)
    {
        Resolver.Log.Info($"{direction}");

        stepper.Rotate(360f, direction, rate);
        Thread.Sleep(1000);

        direction = direction switch
        {
            RotationDirection.CounterClockwise => RotationDirection.Clockwise,
            _ => RotationDirection.CounterClockwise
        };
    }
}

```
## How to Contribute

- **Found a bug?** [Report an issue](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Have a **feature idea or driver request?** [Open a new feature request](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Want to **contribute code?** Fork the [Meadow.Foundation](https://github.com/WildernessLabs/Meadow.Foundation) repository and submit a pull request against the `develop` branch


## Need Help?

If you have questions or need assistance, please join the Wilderness Labs [community on Slack](http://slackinvite.wildernesslabs.co/).
