# Meadow.Foundation.Sensors.Motion.ParallaxPir

**Parallax PIR analog motion sensor**

The **ParallaxPir** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/)

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/), to view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/)

## Usage

```csharp
private ParallaxPir parallaxPir;

public override Task Initialize()
{
    Resolver.Log.Info("Initialize...");

    parallaxPir = new ParallaxPir(Device.CreateDigitalInterruptPort(Device.Pins.D05, InterruptMode.EdgeBoth, ResistorMode.Disabled));

    parallaxPir.OnMotionStart += (sender) => Resolver.Log.Info($"Motion start  {DateTime.Now}");
    parallaxPir.OnMotionEnd += (sender) => Resolver.Log.Info($"Motion end  {DateTime.Now}");

    return Task.CompletedTask;
}

```
