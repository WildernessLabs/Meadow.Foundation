# Meadow.Foundation.ICs.IOExpanders.Pca9671

**PCA9671 I2C digital input/output expander**

The **Pca9671** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/).

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Usage

```csharp
private Pca9671 pca;

public override Task Initialize()
{
    pca = new Pca9671(Device.CreateI2cBus(), 0x20, Device.Pins.D01);

    return base.Initialize();
}

public override Task Run()
{
    while (true)
    {
        //TestBulkDigitalOutputPortWrites(20);
        TestDigitalOutputPorts(2);
    }
}

private void TestDigitalOutputPorts(int loopCount)
{
    var out00 = pca.CreateDigitalOutputPort(pca.Pins.R00);
    var out01 = pca.CreateDigitalOutputPort(pca.Pins.R01);
    var out02 = pca.CreateDigitalOutputPort(pca.Pins.R02);
    var out03 = pca.CreateDigitalOutputPort(pca.Pins.R03);
    var out04 = pca.CreateDigitalOutputPort(pca.Pins.R04);
    var out05 = pca.CreateDigitalOutputPort(pca.Pins.R05);
    var out06 = pca.CreateDigitalOutputPort(pca.Pins.R06);
    var out07 = pca.CreateDigitalOutputPort(pca.Pins.R07);
    var out08 = pca.CreateDigitalOutputPort(pca.Pins.R08);
    var out09 = pca.CreateDigitalOutputPort(pca.Pins.R09);
    var out10 = pca.CreateDigitalOutputPort(pca.Pins.R10);
    var out11 = pca.CreateDigitalOutputPort(pca.Pins.R11);
    var out12 = pca.CreateDigitalOutputPort(pca.Pins.R12);
    var out13 = pca.CreateDigitalOutputPort(pca.Pins.R13);
    var out14 = pca.CreateDigitalOutputPort(pca.Pins.R14);
    var out15 = pca.CreateDigitalOutputPort(pca.Pins.R15);

    var outputPorts = new List<IDigitalOutputPort>()
    {
        out00, out01, out02, out03, out04, out05, out06, out07,
        out08, out09, out10, out11, out12, out13, out14, out15
    };

    foreach (var outputPort in outputPorts)
    {
        outputPort.State = true;
    }

    for (int l = 0; l < loopCount; l++)
    {
        // loop through all the outputs
        for (int i = 0; i < outputPorts.Count; i++)
        {
            // turn them all off
            pca.AllOff();

            // turn on just one
            outputPorts[i].State = true;
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
