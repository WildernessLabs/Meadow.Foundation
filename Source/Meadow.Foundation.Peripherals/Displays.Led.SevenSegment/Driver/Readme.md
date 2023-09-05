# Meadow.Foundation.Displays.Led.SevenSegment

**Digital Seven (7) segment display**

The **SevenSegment** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/)

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/), to view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/)

## Usage

```
SevenSegment sevenSegment;

public override Task Initialize()
{
    Resolver.Log.Info("Initializing...");

    sevenSegment = new SevenSegment
    (
        portA: Device.CreateDigitalOutputPort(Device.Pins.D14),
        portB: Device.CreateDigitalOutputPort(Device.Pins.D15),
        portC: Device.CreateDigitalOutputPort(Device.Pins.D06),
        portD: Device.CreateDigitalOutputPort(Device.Pins.D07),
        portE: Device.CreateDigitalOutputPort(Device.Pins.D08),
        portF: Device.CreateDigitalOutputPort(Device.Pins.D13),
        portG: Device.CreateDigitalOutputPort(Device.Pins.D12),
        portDecimal: Device.CreateDigitalOutputPort(Device.Pins.D05),
        isCommonCathode: false
    );

    return base.Initialize();
}

public override Task Run()
{
    sevenSegment.SetDisplay(character: '1', showDecimal: true);

    return base.Run();
}

```
