# Meadow.Foundation.Sensors.Sound.Ky038

**KY-038 analog sound sensor**

The **Ky038** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/)

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/), to view all of Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/)

## Usage

```
Ky038 sensor;

public override Task Initialize()
{
    Resolver.Log.Info("Initialize...");

    sensor = new Ky038(Device.Pins.A00, Device.Pins.D10);

    return Task.CompletedTask;
}

public override Task Run()
{
    return Task.CompletedTask;
}

        
```

