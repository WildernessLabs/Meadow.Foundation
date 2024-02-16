# Meadow.Foundation.ICs.IOExpanders.Pcx857x

**Pcx857x I2C digital input/output expander (Pca8574, Pca8575, Pcf8574, Pcf855)**

The **Pcx857x** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/).

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Usage

```csharp
private Pca8574 device;

public override Task Initialize()
{
    device = new Pca8574(Device.CreateI2cBus(1), 0x20, Device.Pins.D01);

    return base.Initialize();
}

public override Task Run()
{
    TestDigitalOutputPorts(10);

    return Task.CompletedTask;
}

private void TestDigitalOutputPorts(int loopCount)
{
    var out00 = device.CreateDigitalOutputPort(device.Pins.P0);
    var out01 = device.CreateDigitalOutputPort(device.Pins.P1);
    var out02 = device.CreateDigitalOutputPort(device.Pins.P2);
    var out03 = device.CreateDigitalOutputPort(device.Pins.P3);
    var out04 = device.CreateDigitalOutputPort(device.Pins.P4);
    var out05 = device.CreateDigitalOutputPort(device.Pins.P5);
    var out06 = device.CreateDigitalOutputPort(device.Pins.P6);
    var out07 = device.CreateDigitalOutputPort(device.Pins.P7);

    var outputPorts = new List<IDigitalOutputPort>()
    {
        out00, out01, out02, out03,
        out04, out05, out06, out07,
    };

    foreach (var outputPort in outputPorts)
    {
        outputPort.State = true;

        Thread.Sleep(100);
    }

    for (int l = 0; l < loopCount; l++)
    {
        // loop through all the outputs
        for (int i = 0; i < outputPorts.Count; i++)
        {
            // turn them all off
            device.AllOff();

            Thread.Sleep(1000);

            // turn them all on
            device.AllOn();
            Thread.Sleep(1000);

            // turn on just one
            Resolver.Log.Info($"Update pin {i} to {true}");
            outputPorts[i].State = true;
            Thread.Sleep(250);

            // turn off just one
            Resolver.Log.Info($"Update pin {i} to {false}");
            outputPorts[i].State = false;
            Thread.Sleep(250);
        }
    }

    // cleanup
    for (int i = 0; i < outputPorts.Count; i++)
    {
        outputPorts[i].Dispose();
    }
}
        
```
## How to Contribute

- **Found a bug?** [Report an issue](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Have a **feature idea or driver request?** [Open a new feature request](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Want to **contribute code?** Fork the [Meadow.Foundation](https://github.com/WildernessLabs/Meadow.Foundation) repository and submit a pull request against the `develop` branch


## Need Help?

If you have questions or need assistance, please join the Wilderness Labs [community on Slack](http://slackinvite.wildernesslabs.co/).
