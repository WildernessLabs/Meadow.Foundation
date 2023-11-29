# Meadow.Foundation.Motors.ElectronicSpeedController

**PWM Electronic speed controller**

The **ElectronicSpeedController** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/).

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Usage

```csharp
private readonly Frequency frequency = new Frequency(50, Frequency.UnitType.Hertz);
private const float armMs = 0.5f;
private const float powerIncrement = 0.05f;
private ElectronicSpeedController esc;
private RotaryEncoderWithButton rotary;

public override Task Initialize()
{
    Resolver.Log.Info("Initialize...");

    rotary = new RotaryEncoderWithButton(Device.Pins.D07, Device.Pins.D08, Device.Pins.D06);
    rotary.Rotated += RotaryRotated;
    rotary.Clicked += (s, e) =>
    {
        Resolver.Log.Info($"Arming the device.");
        esc.Arm();
    }; ;

    esc = new ElectronicSpeedController(Device.Pins.D02, frequency);

    Resolver.Log.Info("Hardware initialized.");

    return base.Initialize();
}

private void RotaryRotated(object sender, RotaryChangeResult e)
{
    esc.Power += (e.New == RotationDirection.Clockwise) ? powerIncrement : -powerIncrement;
    DisplayPowerOnLed(esc.Power);

    Resolver.Log.Info($"New Power: {esc.Power * 100:n0}%");
}

/// <summary>
/// Displays the ESC power on the onboard LED as full red @ `100%`,
/// blue @ `0%`, and a proportional mix, in between those speeds.
/// </summary>
/// <param name="power"></param>
private void DisplayPowerOnLed(float power)
{
    // `0.0` - `1.0`
    int r = (int)ExtensionMethods.Map(power, 0f, 1f, 0f, 255f);
    int b = (int)ExtensionMethods.Map(power, 0f, 1f, 255f, 0f);

    var color = Color.FromRgb(r, 0, b);
}

public override Task Run()
{
    DisplayPowerOnLed(esc.Power);

    return base.Run();
}

```
## How to Contribute

- **Found a bug?** [Report an issue](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Have a **feature idea or driver request?** [Open a new feature request](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Want to **contribute code?** Fork the [Meadow.Foundation](https://github.com/WildernessLabs/Meadow.Foundation) repository and submit a pull request against the `develop` branch


## Need Help?

If you have questions or need assistance, please join the Wilderness Labs [community on Slack](http://slackinvite.wildernesslabs.co/).
