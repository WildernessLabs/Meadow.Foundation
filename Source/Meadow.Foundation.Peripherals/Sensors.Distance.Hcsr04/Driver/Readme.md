# Meadow.Foundation.Sensors.Distance.Hcsr04

**HCSR04 digital ultrasonic distance sensor**

The **Hcsr04** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/)

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/), to view all of Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/)

## Usage

```
Hcsr04 hCSR04;

public override Task Initialize()
{
    Resolver.Log.Info($"Hello HC-SR04 sample");

    hCSR04 = new Hcsr04(
        triggerPin: Device.Pins.D05,
        echoPin: Device.Pins.D06);
    hCSR04.DistanceUpdated += HCSR04_DistanceUpdated;

    return Task.CompletedTask;
}

public override Task Run()
{
    while (true)
    {
        // Sends a trigger signal
        hCSR04.MeasureDistance();
        Thread.Sleep(2000);
    }
}

private void HCSR04_DistanceUpdated(object sender, IChangeResult<Meadow.Units.Length> e)
{
    Resolver.Log.Info($"Distance (cm): {e.New.Centimeters}");
}

        
```

