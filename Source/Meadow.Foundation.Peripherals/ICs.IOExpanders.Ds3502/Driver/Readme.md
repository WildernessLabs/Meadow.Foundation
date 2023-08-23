# Meadow.Foundation.ICs.IOExpanders.Ds3502

**Ds3502 I2C digital potentiometer**

The **Ds3502** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/)

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/), to view all of Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/)

## Usage

```
protected Ds3502 ds3502;

public override Task Initialize()
{
    Resolver.Log.Info("Initialize...");

    ds3502 = new Ds3502(Device.CreateI2cBus(Ds3502.DefaultBusSpeed));

    return base.Initialize();
}

public override Task Run()
{
    for (byte i = 0; i < 127; i++)
    {
        ds3502.SetWiper(i);
        Resolver.Log.Info($"wiper {ds3502.GetWiper()}");

        Thread.Sleep(1000);
    }

    return base.Run();
}

        
```

