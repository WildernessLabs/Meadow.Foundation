# Meadow.Foundation.Displays.Led.SixteenSegment

**Digital Sixteen (16) segment display**

The **SixteenSegment** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/).

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Usage

```csharp
SixteenSegment sixteenSegment;

public override Task Initialize()
{
    Resolver.Log.Info("Initializing...");

    sixteenSegment = new SixteenSegment
    (
        portA: Device.CreateDigitalOutputPort(Device.Pins.D00),
        portB: Device.CreateDigitalOutputPort(Device.Pins.D01),
        portC: Device.CreateDigitalOutputPort(Device.Pins.D02),
        portD: Device.CreateDigitalOutputPort(Device.Pins.D03),
        portE: Device.CreateDigitalOutputPort(Device.Pins.D04),
        portF: Device.CreateDigitalOutputPort(Device.Pins.D05),
        portG: Device.CreateDigitalOutputPort(Device.Pins.D06),
        portH: Device.CreateDigitalOutputPort(Device.Pins.D07),
        portK: Device.CreateDigitalOutputPort(Device.Pins.D08),
        portM: Device.CreateDigitalOutputPort(Device.Pins.D09),
        portN: Device.CreateDigitalOutputPort(Device.Pins.D10),
        portP: Device.CreateDigitalOutputPort(Device.Pins.D11),
        portR: Device.CreateDigitalOutputPort(Device.Pins.D12),
        portS: Device.CreateDigitalOutputPort(Device.Pins.D13),
        portT: Device.CreateDigitalOutputPort(Device.Pins.D14),
        portU: Device.CreateDigitalOutputPort(Device.Pins.D15),
        portDecimal: Device.CreateDigitalOutputPort(Device.Pins.A00),
        isCommonCathode: false
    );

    return base.Initialize();
}

public override Task Run()
{
    sixteenSegment.SetDisplay(asciiCharacter: 'Z', showDecimal: true);

    return base.Run();
}

```
## How to Contribute

- **Found a bug?** [Report an issue](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Have a **feature idea or driver request?** [Open a new feature request](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Want to **contribute code?** Fork the [Meadow.Foundation](https://github.com/WildernessLabs/Meadow.Foundation) repository and submit a pull request against the `develop` branch


## Need Help?

If you have questions or need assistance, please join the Wilderness Labs [community on Slack](http://slackinvite.wildernesslabs.co/).
