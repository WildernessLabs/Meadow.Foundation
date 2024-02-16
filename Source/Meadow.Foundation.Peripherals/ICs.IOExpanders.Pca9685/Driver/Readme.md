# Meadow.Foundation.ICs.IOExpanders.Pca9685

**PCA9685 I2C PWM expander**

The **Pca9685** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/).

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Usage

```csharp
Pca9685 pca9685;

public override Task Initialize()
{
    Resolver.Log.Info("Initialize...");
    var i2CBus = Device.CreateI2cBus(I2cBusSpeed.FastPlus);

    pca9685 = new Pca9685(i2CBus, new Meadow.Units.Frequency(50, Meadow.Units.Frequency.UnitType.Hertz), (byte)Pca9685.Addresses.Default);
    pca9685.Initialize();

    return base.Initialize();
}

public override Task Run()
{
    var port0 = pca9685.CreatePwmPort(0, 0.05f);
    var port7 = pca9685.CreatePwmPort(7);

    port0.Start();
    port7.Start();

    return base.Run();
}

```
## How to Contribute

- **Found a bug?** [Report an issue](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Have a **feature idea or driver request?** [Open a new feature request](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Want to **contribute code?** Fork the [Meadow.Foundation](https://github.com/WildernessLabs/Meadow.Foundation) repository and submit a pull request against the `develop` branch


## Need Help?

If you have questions or need assistance, please join the Wilderness Labs [community on Slack](http://slackinvite.wildernesslabs.co/).
