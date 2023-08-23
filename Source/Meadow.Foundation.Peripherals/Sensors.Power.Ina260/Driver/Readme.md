# Meadow.Foundation.Sensors.Power.Ina260

**INA260 I2C current and power monitor**

The **Ina260** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/)

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/), to view all of Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/)

## Usage

```
Ina260 ina260;

public override Task Initialize()
{
    Resolver.Log.Info("Initialize...");

    var bus = Device.CreateI2cBus();
    ina260 = new Ina260(bus);

    Resolver.Log.Info($"-- INA260 Sample App ---");
    Resolver.Log.Info($"Manufacturer: {ina260.ManufacturerID}");
    Resolver.Log.Info($"Die: {ina260.DieID}");
    ina260.Updated += (s, v) =>
    {
        Resolver.Log.Info($"{v.New.Item2}V @ {v.New.Item3}A");
    };

    return Task.CompletedTask;
}

public override Task Run()
{ 
    ina260.StartUpdating(TimeSpan.FromSeconds(2));
    return Task.CompletedTask;
}

        
```

