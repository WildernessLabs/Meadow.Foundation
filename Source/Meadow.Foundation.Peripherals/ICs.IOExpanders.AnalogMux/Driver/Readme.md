# Meadow.Foundation.ICs.IOExpanders.AnalogMux

**Library for various analog multiplexers**

The **AnalogMux** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/).

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Usage

```csharp
private Nxp74HC4051 mux;

public override Task Initialize()
{
    Resolver.Log.Info("Initialize...");

    mux = new Nxp74HC4051(
        Device.CreateAnalogInputPort(Device.Pins.A00),      // input
        Device.CreateDigitalOutputPort(Device.Pins.D00),    // s0
        Device.CreateDigitalOutputPort(Device.Pins.D01),    // s1
        Device.CreateDigitalOutputPort(Device.Pins.D02),    // s2
        Device.CreateDigitalOutputPort(Device.Pins.D03)     // enable
        );

    return base.Initialize();
}

public override Task Run()
{
    Task.Run(ReadRoundRobin);

    return base.Run();
}

public async Task ReadRoundRobin()
{
    while (true)
    {
        for (var channel = 0; channel < 8; channel++)
        {
            mux.SetInputChannel(channel);
            var read = await mux.Signal.Read();
            Resolver.Log.Info($"ADC Channel {channel} = {read.Volts:0.0}V");
            await Task.Delay(1000);
        }
    }
}

```
## How to Contribute

- **Found a bug?** [Report an issue](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Have a **feature idea or driver request?** [Open a new feature request](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Want to **contribute code?** Fork the [Meadow.Foundation](https://github.com/WildernessLabs/Meadow.Foundation) repository and submit a pull request against the `develop` branch


## Need Help?

If you have questions or need assistance, please join the Wilderness Labs [community on Slack](http://slackinvite.wildernesslabs.co/).
