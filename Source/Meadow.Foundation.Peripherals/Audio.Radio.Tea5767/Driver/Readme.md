# Meadow.Foundation.Audio.Radio.Tea5767

**TEA5767 I2C FM radio module**

The **Tea5767** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/)

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/), to view all of Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/)

## Usage

```
Tea5767 radio;

public override Task Initialize()
{
    Resolver.Log.Info("Initializing...");
    
    radio = new Tea5767(Device.CreateI2cBus());

    return Task.CompletedTask;
}

public async override Task Run()
{
    //scan through avaliable stations
    for (int i = 0; i < 8; i++)
    {
        await Task.Delay(1000);

        radio.SearchNextSilent();

        Resolver.Log.Info($"Current frequency: {radio.GetFrequency()}");
    }

    //set a known station
    radio.SelectFrequency(new Frequency(94.5, Frequency.UnitType.Megahertz));
}

        
```

