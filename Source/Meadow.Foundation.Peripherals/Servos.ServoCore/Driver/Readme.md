# Meadow.Foundation.Servos.ServoCore

**PWM generic servo controller**

The **ServoCore** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/)

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/), to view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/)

## Usage

```
protected Servo servo;

public override Task Initialize()
{
    Resolver.Log.Info("Initialize...");

    servo = new Servo(Device.Pins.D02, NamedServoConfigs.SG90);

    return Task.CompletedTask;
}

public async override Task Run()
{ 
    await servo.RotateTo(new Angle(0, AU.Degrees));

    while (true)
    {
        for (int i = 0; i <= servo.Config.MaximumAngle.Degrees; i++)
        {
            await servo.RotateTo(new Angle(i, AU.Degrees));
            Resolver.Log.Info($"Rotating to {i}");
        }

        await Task.Delay(2000);

        for (int i = 180; i >= servo.Config.MinimumAngle.Degrees; i--)
        {
            await servo.RotateTo(new Angle(i, AU.Degrees));
            Resolver.Log.Info($"Rotating to {i}");
        }
        await Task.Delay(2000);
    }
}

```
