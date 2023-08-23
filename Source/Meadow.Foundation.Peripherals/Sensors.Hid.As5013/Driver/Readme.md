# Meadow.Foundation.Sensors.Hid.As5013

**I2C Hall sensor IC for smart navigation**

The **As5013** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/)

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/), to view all of Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/)

## Usage

```
As5013 joystick;

public override Task Initialize()
{
    Resolver.Log.Info("Initializing ...");

    joystick = new As5013(Device.CreateI2cBus());

    joystick.Updated += As5013_Updated;

    return Task.CompletedTask;
}

public override Task Run()
{
    joystick.StartUpdating(TimeSpan.FromMilliseconds(100));

    return Task.CompletedTask;
}

private void As5013_Updated(object sender, IChangeResult<Meadow.Peripherals.Sensors.Hid.AnalogJoystickPosition> e)
{
    Resolver.Log.Info($"{e.New.Horizontal}, {e.New.Vertical}");
}

        
```

