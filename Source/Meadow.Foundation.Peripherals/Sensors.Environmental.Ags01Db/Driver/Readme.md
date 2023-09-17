# Meadow.Foundation.Sensors.Environmental.Ags01Db

**Ags01Db I2C digital MEMS VOC gas sensor**

The **Ags01Db** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/)

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/), to view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/)

## Usage

```csharp
Ags01Db ags10Db;

public override Task Initialize()
{
    Resolver.Log.Info("Initialize ...");
    ags10Db = new Ags01Db(Device.CreateI2cBus());

    Resolver.Log.Info($"Version: v{ags10Db.GetVersion()}");

    var consumer = Ags01Db.CreateObserver(
        handler: result =>
        {
            Resolver.Log.Info($"Concentration New Value {result.New.PartsPerMillion}ppm");
            Resolver.Log.Info($"Concentration Old Value {result.Old?.PartsPerMillion}ppm");
        },
        filter: null
    );
    ags10Db.Subscribe(consumer);

    ags10Db.ConcentrationUpdated += (object sender, IChangeResult<Meadow.Units.Concentration> e) =>
    {
        Resolver.Log.Info($"Concentration Updated: {e.New.PartsPerMillion:N2}ppm");
    };

    return Task.CompletedTask;
}

public override Task Run()
{
    ags10Db.StartUpdating(TimeSpan.FromSeconds(1));

    return Task.CompletedTask;
}

```
