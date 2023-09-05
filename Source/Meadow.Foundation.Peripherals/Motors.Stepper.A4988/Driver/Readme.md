# Meadow.Foundation.Motors.Stepper.A4988

**A4988 digital input stepper motor controller**

The **A4988** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/)

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/), to view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/)

## Usage

```
A4988 a4988;

public override Task Initialize()
{
    a4988 = new A4988(
        step: Device.Pins.D01,
        direction: Device.Pins.D00,
        ms1Pin: Device.Pins.D04,
        ms2Pin: Device.Pins.D03,
        ms3Pin: Device.Pins.D02);

    return base.Initialize();
}

public override Task Run()
{
    var stepDivisors = (StepDivisor[])Enum.GetValues(typeof(StepDivisor));
    while (true)
    {
        foreach (var step in stepDivisors)
        {
            for (var d = 2; d < 5; d++)
            {
                Resolver.Log.Info($"180 degrees..Speed divisor = {d}..1/{(int)step} Steps..{a4988.Direction}...");
                a4988.RotationSpeedDivisor = d;
                a4988.StepDivisor = step;
                a4988.Rotate(180);

                Thread.Sleep(500);
            }
        }
        a4988.Direction = (a4988.Direction == RotationDirection.Clockwise) ? RotationDirection.CounterClockwise : RotationDirection.Clockwise;
    }
}

```
