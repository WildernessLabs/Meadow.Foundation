# Meadow.Foundation.Sensors.Distance.A02yyuw

**A02yyuw serial ultrasonic distance sensor**

The **A02yyuw** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/).

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Usage

```csharp
A02yyuw a02yyuw;

public override Task Initialize()
{
    Resolver.Log.Info("Initialize...");

    a02yyuw = new A02yyuw(Device, Device.PlatformOS.GetSerialPortName("COM1"));

    var consumer = A02yyuw.CreateObserver(
        handler: result =>
        {
            Resolver.Log.Info($"Observer: Distance changed by threshold; new distance: {result.New.Centimeters:N1}cm, old: {result.Old?.Centimeters:N1}cm");
        },
        filter: result =>
        {
            if (result.Old is { } old)
            {
                return Math.Abs((result.New - old).Centimeters) > 0.5;
            }
            return false;
        }
    );
    a02yyuw.Subscribe(consumer);

    a02yyuw.Updated += A02yyuw_DistanceUpdated;

    return Task.CompletedTask;
}

public override async Task Run()
{
    Resolver.Log.Info("Run...");

    var distance = await a02yyuw.Read();
    Resolver.Log.Info($"Initial distance is: {distance.Centimeters:N1}cm / {distance.Inches:N1}in");

    a02yyuw.StartUpdating(TimeSpan.FromSeconds(2));
}

private void A02yyuw_DistanceUpdated(object sender, IChangeResult<Length> e)
{
    Resolver.Log.Info($"Distance: {e.New.Centimeters:N1}cm / {e.New.Inches:N1}in");
}

```
## How to Contribute

- **Found a bug?** [Report an issue](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Have a **feature idea or driver request?** [Open a new feature request](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Want to **contribute code?** Fork the [Meadow.Foundation](https://github.com/WildernessLabs/Meadow.Foundation) repository and submit a pull request against the `develop` branch


## Need Help?

If you have questions or need assistance, please join the Wilderness Labs [community on Slack](http://slackinvite.wildernesslabs.co/).
