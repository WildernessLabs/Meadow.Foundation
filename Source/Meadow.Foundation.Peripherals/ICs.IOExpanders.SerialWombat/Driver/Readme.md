# Meadow.Foundation.ICs.IOExpanders.SerialWombat

**SerialWombat I2C IO expander for GPIO, PWM, servos, etc.**

The **SerialWombat** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/).

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Usage

```csharp
private Sw18AB serialWombat;
private IAnalogInputPort analogInputPort;

public override Task Initialize()
{
    Resolver.Log.Info("Initialize...");

    try
    {
        Resolver.Log.Info(" creating Wombat...");
        serialWombat = new Sw18AB(Device.CreateI2cBus(), logger: Resolver.Log);
        Resolver.Log.Info(" creating ADC...");
        analogInputPort = serialWombat.CreateAnalogInputPort(serialWombat.Pins.WP0);
    }
    catch (Exception ex)
    {
        Resolver.Log.Error($"error: {ex.Message}");
    }

    return Task.CompletedTask;
}

public override async Task Run()
{
    Resolver.Log.Info("Running...");

    Resolver.Log.Info($"ADC: Reference voltage = {analogInputPort.ReferenceVoltage.Volts:0.0}V");
    for (int i = 1; i < 1000; i += 10)
    {
        var v = await analogInputPort.Read();
        Resolver.Log.Info($"ADC: {analogInputPort.Voltage.Volts:0.0}V");
        await Task.Delay(2000);
    }
}

```
## How to Contribute

- **Found a bug?** [Report an issue](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Have a **feature idea or driver request?** [Open a new feature request](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Want to **contribute code?** Fork the [Meadow.Foundation](https://github.com/WildernessLabs/Meadow.Foundation) repository and submit a pull request against the `develop` branch


## Need Help?

If you have questions or need assistance, please join the Wilderness Labs [community on Slack](http://slackinvite.wildernesslabs.co/).
