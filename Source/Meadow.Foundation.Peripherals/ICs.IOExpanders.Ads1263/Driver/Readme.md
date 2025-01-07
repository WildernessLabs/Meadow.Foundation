# Meadow.Foundation.ICs.IOExpanders.Ads1263

**TI ADS1263 SPI analog to digital converter, IO expander**

The **Ads1263** library is included in the **Meadow.Foundation.ICs.IOExpanders.Ads1263** nuget package and is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform.

This driver is part of the [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/) peripherals library, an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT applications.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Installation

You can install the library from within Visual studio using the the NuGet Package Manager or from the command line using the .NET CLI:

`dotnet add package Meadow.Foundation.ICs.IOExpanders.Ads1263`
## Usage

```csharp
private Ads1263 ads1263;
private Ads1263.AnalogInputPort adc1, adc2;
private Ads1263.DigitalInputPort gpio0, gpio1;
private Ads1263.DigitalOutputPort gpio6, gpio7;

public override Task Initialize()
{
    Resolver.Log.Info("Initialize...");

    var spiBus = Device.CreateSpiBus();
    ads1263 = new Ads1263(spiBus, Device.Pins.A05);

    // Setup ADC1 on the default inputs
    ads1263.ConfigureADC1(positiveSource: Ads1263.AdcSource.AIN0, negativeSource: Ads1263.AdcSource.AIN1);
    adc1 = (Ads1263.AnalogInputPort)ads1263.CreateAnalogInputPort(ads1263.Pins.ADC1, 1, TimeSpan.Zero, 2.5.Volts());
    adc1.StartConversions();
    adc1.Updated += Adc1_Updated;
    adc1.StartUpdating(TimeSpan.FromSeconds(5));

    // Setup ADC2 to read the internal temperature
    ads1263.ConfigureADC2(positiveSource: Ads1263.AdcSource.TempSensor, negativeSource: Ads1263.AdcSource.TempSensor);
    adc2 = (Ads1263.AnalogInputPort)ads1263.CreateAnalogInputPort(ads1263.Pins.ADC2, 1, TimeSpan.Zero, 2.5.Volts());
    adc2.StartConversions();
    adc2.Updated += Adc2_Updated;
    adc2.StartUpdating(TimeSpan.FromSeconds(5));

    // Setup digital inputs and outputs
    gpio0 = (Ads1263.DigitalInputPort)ads1263.CreateDigitalInputPort(ads1263.Pins.GPIO0);
    gpio1 = (Ads1263.DigitalInputPort)ads1263.CreateDigitalInputPort(ads1263.Pins.GPIO1);
    gpio6 = (Ads1263.DigitalOutputPort)ads1263.CreateDigitalOutputPort(ads1263.Pins.GPIO6);
    gpio7 = (Ads1263.DigitalOutputPort)ads1263.CreateDigitalOutputPort(ads1263.Pins.GPIO7);

    return base.Initialize();
}

private void Adc1_Updated(object sender, IChangeResult<Voltage> e)
{
    Resolver.Log.Info($"ADC1 {e.New.Volts:N6} V");
}

private void Adc2_Updated(object sender, IChangeResult<Voltage> e)
{
    Resolver.Log.Info($"ADC2 {e.New.Volts:N6} V = {(Ads1263.ConvertTempSensor(e.New)):N2} °C");
}

public override Task Run()
{
    Resolver.Log.Info("Run...");

    while (true)
    {
        gpio7.State = gpio0.State;
        gpio6.State = !gpio1.State;

        Thread.Sleep(TimeSpan.FromSeconds(1));
    }
}

```
## How to Contribute

- **Found a bug?** [Report an issue](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Have a **feature idea or driver request?** [Open a new feature request](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Want to **contribute code?** Fork the [Meadow.Foundation](https://github.com/WildernessLabs/Meadow.Foundation) repository and submit a pull request against the `develop` branch


## Need Help?

If you have questions or need assistance, please join the Wilderness Labs [community on Slack](http://slackinvite.wildernesslabs.co/).
## About Meadow

Meadow is a complete, IoT platform with defense-grade security that runs full .NET applications on embeddable microcontrollers and Linux single-board computers including Raspberry Pi and NVIDIA Jetson.

### Build

Use the full .NET platform and tooling such as Visual Studio and plug-and-play hardware drivers to painlessly build IoT solutions.

### Connect

Utilize native support for WiFi, Ethernet, and Cellular connectivity to send sensor data to the Cloud and remotely control your peripherals.

### Deploy

Instantly deploy and manage your fleet in the cloud for OtA, health-monitoring, logs, command + control, and enterprise backend integrations.


