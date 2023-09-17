# Meadow.Foundation.ICs.FanControllers.Emc2101

**Emc2101 I2C fan controller and temperature monitor**

The **Emc2101** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/)

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/), to view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/)

## Usage

```csharp
Emc2101 fanController;

public override Task Initialize()
{
    Resolver.Log.Info("Initialize...");

    fanController = new Emc2101(i2cBus: Device.CreateI2cBus());

    return base.Initialize();
}

public override Task Run()
{
    Resolver.Log.Info("Run ...");

    return base.Run();
}

```
