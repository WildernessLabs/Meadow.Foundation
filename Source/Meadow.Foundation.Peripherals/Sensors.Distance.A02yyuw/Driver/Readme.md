# Meadow.Foundation.Sensors.Distance.A02yyuw

**A02yyuw serial ultrasonic distance sensor**

The A02 series sensor module is an ultrasonic distance sensor that reports distance to objects over a range from 30 millimetres to 4.4 metres. The A02 series supports 5 different modes of operation (PWM output, UART Controlled output, UART Auto output, Switched output). The Rx and Tx pin function corresponds to the output mode selected before ordering, and can not be changed. The A02yyuw library supports the UART Controlled output and UART Auto output of the A02. These two modes use a UART serial interface, typically COM4 or COM1 on the Meadow boards. Pinout is as follows:

### Pinout for Sensor:
| Pin | Color  | Description                             |
|-----|--------|-----------------------------------------|
|  1  |  Red   | VCC power input 3- 5 V                  |
|  2  | Black  | Ground                                  |
|  3  | Yellow | Rx pin function depends on output mode  |
|  4  | White  | Tx pin function depends on output mode  |

Note that Rx and Tx are defined from the perspective of the A02 module, so are reversed from the perspective of the Meadow board.


In UART Auto mode, when the Rx pin is held low, the module continuously outputs real-time values on the Tx pin with a response time of 100ms.
When pin(RX)is held high, the module outputs processed values, and the data is more stable. The response time is 100~500ms. The A02yyuw library
keeps Rx high in UART Auto output, so processed values are obtained.

In UART Controlled mode, the module will perform a distance detection and send data on the Tx pin after the Rx pin receives a falling edge pulse.
The period between Rx pulses must be greater than 70ms. The **A02yyuw** library sends a pulse by writing a byte of data to the serial port.

The data from sent from the sensor is in milimetres and comes as a 2 byte unsigned integer.  Each sensor reading is packed in a data frame as follows:

### Data Frame Description:
| Byte | Data Frame | Description     |
|------|------------|-----------------|
| 1    | Start Byte | 0xFF 0xFF       |
| 2    | Data_H     | High 8 Distance |
| 3    | Data_L     | Low 8 Distance  |
| 4    | SUM        | Parity Sum      |

The distance in millimeters = Data_H * 256 + Data_L. The check sum is the lower 8 bits of the sum of the first three bytes, SUM =(start bit+ Data_H + Data_L) & 0x00FF.

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

    a02yyuw = new A02yyuw(Device, Device.PlatformOS.GetSerialPortName("COM4"), A02yyuw.MODE_UART_AUTO);

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
