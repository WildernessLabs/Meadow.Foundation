# Meadow.Foundation.Sensors.Temperature.Mcp9808

**MCP9808 I2C temperature sensor**

The **Mcp9808** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/).

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Usage

```csharp
Mcp9808 mcp9808;

public override Task Initialize()
{
    Resolver.Log.Info("Initialize...");

    mcp9808 = new Mcp9808(Device.CreateI2cBus());

    var consumer = Mcp9808.CreateObserver(
        handler: result =>
        {
            Resolver.Log.Info($"Temperature New Value {result.New.Celsius}C");
            Resolver.Log.Info($"Temperature Old Value {result.Old?.Celsius}C");
        },
        filter: null
    );
    mcp9808.Subscribe(consumer);

    mcp9808.Updated += (object sender, IChangeResult<Meadow.Units.Temperature> e) =>
    {
        Resolver.Log.Info($"Temperature Updated: {e.New.Celsius:N2}C");
    };

    return Task.CompletedTask;
}

public override async Task Run()
{
    var temp = await mcp9808.Read();

    Resolver.Log.Info($"Temperature New Value {temp.Celsius}C");

    mcp9808.StartUpdating(TimeSpan.FromSeconds(1));
}

```
## How to Contribute

- **Found a bug?** [Report an issue](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Have a **feature idea or driver request?** [Open a new feature request](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Want to **contribute code?** Fork the [Meadow.Foundation](https://github.com/WildernessLabs/Meadow.Foundation) repository and submit a pull request against the `develop` branch


## Need Help?

If you have questions or need assistance, please join the Wilderness Labs [community on Slack](http://slackinvite.wildernesslabs.co/).
