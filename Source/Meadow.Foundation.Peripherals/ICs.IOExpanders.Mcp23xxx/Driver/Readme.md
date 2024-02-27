# Meadow.Foundation.ICs.IOExpanders.Mcp23xxx

**Mcp23xxx I2C and SPI digital output expanders**

The **Mcp23xxx** library is included in the **Meadow.Foundation.ICs.IOExpanders.Mcp23xxx** nuget package and is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform.

This driver is part of the [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/) peripherals library, an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT applications.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Installation

You can install the library from within Visual studio using the the NuGet Package Manager or from the command line using the .NET CLI:

`dotnet add package Meadow.Foundation.ICs.IOExpanders.Mcp23xxx`
## Usage

```csharp
private Mcp23008 mcp;

public override Task Initialize()
{
    var interruptPort = Device.CreateDigitalInterruptPort(Device.Pins.D00, InterruptMode.EdgeRising);
    var resetPort = Device.CreateDigitalOutputPort(Device.Pins.D01);

    mcp = new Mcp23008(Device.CreateI2cBus(), 0x20, interruptPort, resetPort);

    return base.Initialize();
}

public override Task Run()
{
    BenchmarkDigitalOutputPorts();
    while (true)
    {
        TestBulkDigitalOutputPortWrites(20);
        TestDigitalOutputPorts(2);
    }
}

private void BenchmarkDigitalOutputPorts()
{
    var out00 = mcp.CreateDigitalOutputPort(mcp.Pins.GP0);
    var out01 = mcp.CreateDigitalOutputPort(mcp.Pins.GP1);
    var out02 = mcp.CreateDigitalOutputPort(mcp.Pins.GP2);
    var out03 = mcp.CreateDigitalOutputPort(mcp.Pins.GP3);
    var out04 = mcp.CreateDigitalOutputPort(mcp.Pins.GP4);
    var out05 = mcp.CreateDigitalOutputPort(mcp.Pins.GP5);
    var out06 = mcp.CreateDigitalOutputPort(mcp.Pins.GP6);
    var out07 = mcp.CreateDigitalOutputPort(mcp.Pins.GP7);

    var outputPorts = new List<IDigitalOutputPort>()
    {
        out00, out01, out02, out03, out04, out05, out06, out07
    };

    var state = false;
    var stopwatch = new Stopwatch();
    Resolver.Log.Info("Starting benchmark");

    for (var x = 0; x < 10; x++)
    {
        stopwatch.Restart();
        for (var iteration = 0; iteration < 100; iteration++)
        {
            for (var i = 0; i < outputPorts.Count; i++)
            {
                outputPorts[i].State = state;
            }

            state = !state;
        }
        stopwatch.Stop();

        Resolver.Log.Info($"{100 * outputPorts.Count} pins toggled in {stopwatch.ElapsedMilliseconds}ms");
    }

    Resolver.Log.Info("Benchmark finished");
}

private void TestDigitalOutputPorts(int loopCount)
{
    var out00 = mcp.CreateDigitalOutputPort(mcp.Pins.GP0);
    var out01 = mcp.CreateDigitalOutputPort(mcp.Pins.GP1);
    var out02 = mcp.CreateDigitalOutputPort(mcp.Pins.GP2);
    var out03 = mcp.CreateDigitalOutputPort(mcp.Pins.GP3);
    var out04 = mcp.CreateDigitalOutputPort(mcp.Pins.GP4);
    var out05 = mcp.CreateDigitalOutputPort(mcp.Pins.GP5);
    var out06 = mcp.CreateDigitalOutputPort(mcp.Pins.GP6);
    var out07 = mcp.CreateDigitalOutputPort(mcp.Pins.GP7);

    var outputPorts = new List<IDigitalOutputPort>()
    {
        out00, out01, out02, out03, out04, out05, out06, out07
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
            foreach (var outputPort in outputPorts)
            {
                outputPort.State = false;
            }

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

private void TestBulkDigitalOutputPortWrites(int loopCount)
{
    byte mask = 0x0;

    for (int l = 0; l < loopCount; l++)
    {
        for (int i = 0; i < 8; i++)
        {
            mcp.WriteToPorts(mask);
            mask = (byte)(1 << i);
            Thread.Sleep(5);
        }
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


