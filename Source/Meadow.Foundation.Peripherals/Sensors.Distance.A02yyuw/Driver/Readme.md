# Meadow.Foundation.Sensors.Distance.A02yyuw

**A02yyuw serial ultrasonic distance sensor**

### Pinout for Sensor:
1. (red) VCC power input 3- 5 V
2. (black) ground
3. (yellow) Rx pin function depends on output mode
4. (white) Tx pin function depends on output mode

The A02 series supports 5 different modes of operation (PWM output, UART Controlled output,UART Auto output, Switched output).
The Rx and Tx pin function corresponds to the output mode selected before ordering, and can not be changed. The **A02yyuw** library supports the 
UART Controlled output and UART Auto output of the A02, which can be selected in software.

In UART Auto mode, when the Rx pin is held low, the module outputs real-time values on the Tx pin with a response time of 100ms.
When pin(RX)is held high, the module outputs processed values, and the data is more stable. The response time is 100~500ms. The **A02yyuw** library
keeps Rx high in UART Auto output, so processed values are obtained.

In UART Controlled mode, the module wil perform a distance detection and send data on the Tx pin after the Rx pin receives a falling edge pulse.
The period between Rx pulses must be greater than 70ms. The **A02yyuw** library sends a pulse by writing a byte of data to the serial port.

The **A02yyuw** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/).

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Usage

```csharp
A02yyuw a02yyuw;

public override Task Initialize()
{
    Resolver.Log.Info("Initialize...");

    a02yyuw = new A02yyuw(Device, Device.PlatformOS.GetSerialPortName("COM4"));

    var consumer = A02yyuw.CreateObserver(
        handler: result =>
        {
            Resolver.Log.Info($"Observer: Distance changed by threshold; new distance: {result.New.Centimeters:N1}cm, old: {result.Old?.Centimeters:N1}cm");
        },
        filter: result =>
        {
            if (result.Old is { } old)
            {
                return Math.Abs((result.New - old).Centimeters) > 5.0;
            }
            return false;
        }
    );
    a02yyuw.Subscribe(consumer);

    a02yyuw.Updated += A02yyuw_DistanceUpdated;

    return Task.CompletedTask;
}

public override async Task Run()
{
    Resolver.Log.Info("Run...");

    var distance = await a02yyuw.Read();
    Resolver.Log.Info($"Initial distance is: {distance.Centimeters:N1}cm / {distance.Inches:N1}in");

    a02yyuw.StartUpdating(TimeSpan.FromSeconds(2));
}

private void A02yyuw_DistanceUpdated(object sender, IChangeResult<Length> e)
{
    Resolver.Log.Info($"Distance: {e.New.Centimeters:N1}cm / {e.New.Inches:N1}in");
}

```
## How to Contribute

- **Found a bug?** [Report an issue](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Have a **feature idea or driver request?** [Open a new feature request](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Want to **contribute code?** Fork the [Meadow.Foundation](https://github.com/WildernessLabs/Meadow.Foundation) repository and submit a pull request against the `develop` branch


## Need Help?

If you have questions or need assistance, please join the Wilderness Labs [community on Slack](http://slackinvite.wildernesslabs.co/).
