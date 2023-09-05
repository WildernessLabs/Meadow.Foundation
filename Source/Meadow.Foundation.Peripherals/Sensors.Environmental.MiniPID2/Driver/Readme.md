# Meadow.Foundation.Sensors.Environmental.MiniPID2

**MiniPID2 analog photoionisation (PID) VOC sensor**

The **MiniPID2** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/)

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/), to view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/)

## Usage

```
MiniPID2 sensor;

public override Task Initialize()
{
    Resolver.Log.Info("Initializing...");

    sensor = new MiniPID2(Device.Pins.A01, MiniPID2.MiniPID2Type.PPB_WR);

    var consumer = MiniPID2.CreateObserver(
        handler: result =>
        {
            Resolver.Log.Info($"Observer: VOC concentration changed by threshold; new: {result.New.PartsPerBillion:N1}ppm");
        },
        filter: result =>
        {
            if (result.Old is { } oldCon &&
                result.New is { } newCon)
            {
                return Math.Abs((newCon - oldCon).PartsPerMillion) > 10;
            }
            return false;
        }
    );

    sensor?.Subscribe(consumer);

    if (sensor != null)
    {
        sensor.Updated += (sender, result) =>
        {
            Resolver.Log.Info($"  VOC Concentraion: {result.New.PartsPerMillion:N1}ppm");
        };
    }

    sensor?.StartUpdating(TimeSpan.FromSeconds(2));

    return base.Initialize();
}

```
