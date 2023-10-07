# Meadow.Foundation.Sensors.Gnss

**BG95-M3 Cellular / GNSS controller**

The **Bg95M3** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/).

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Usage

```csharp
Bg95M3 bg95M3;

void ProcessGnssPosition(object sender, IGnssResult location)
{
    Resolver.Log.Info("*********************************************");
    Resolver.Log.Info(location.ToString());
    Resolver.Log.Info("*********************************************");  
}
        
public override Task Initialize()
{
    Resolver.Log.Info("Initializing ...");

    var cellAdapter = Device.NetworkAdapters.Primary<ICellNetworkAdapter>();

    IGnssResult[] resultTypes = new IGnssResult[]
    {
        new GnssPositionInfo(),
        new ActiveSatellites(),
        new CourseOverGround(),
        new SatellitesInView(new Satellite[0])
    };

    bg95M3 = new Bg95M3(cellAdapter, TimeSpan.FromMinutes(30), resultTypes);

    bg95M3.GnssDataReceived += ProcessGnssPosition;

    return Task.CompletedTask;
}

public override Task Run()
{
    bg95M3.StartUpdating();

    return Task.CompletedTask;
}

```
## How to Contribute

- **Found a bug?** [Report an issue](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Have a **feature idea or driver request?** [Open a new feature request](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Want to **contribute code?** Fork the [Meadow.Foundation](https://github.com/WildernessLabs/Meadow.Foundation) repository and submit a pull request against the `develop` branch


## Need Help?

If you have questions or need assistance, please join the Wilderness Labs [community on Slack](http://slackinvite.wildernesslabs.co/).
