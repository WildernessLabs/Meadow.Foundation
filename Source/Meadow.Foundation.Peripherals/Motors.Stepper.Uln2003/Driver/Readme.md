# Meadow.Foundation.Motors.Stepper.Uln2003

**ULN2003 digital input stepper motor controller**

The **Uln2003** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/).

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Usage

```csharp
Uln2003 stepperController;

public override Task Initialize()
{
    stepperController = new Uln2003(
        pin1: Device.Pins.D01,
        pin2: Device.Pins.D02,
        pin3: Device.Pins.D03,
        pin4: Device.Pins.D04);

    return base.Initialize();
}

public override Task Run()
{
    stepperController.Step(1024);

    for (int i = 0; i < 100; i++)
    {
        Resolver.Log.Info($"Step forward {i}");
        stepperController.Step(50);
        Thread.Sleep(10);
    }

    for (int i = 0; i < 100; i++)
    {
        Resolver.Log.Info($"Step backwards {i}");
        stepperController.Step(-50);
        Thread.Sleep(10);
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
