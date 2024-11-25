# Meadow.Foundation.ICs.CAN.Mcp2515

**Microchip MCP2515 CAN Controller**

The **Mcp2515** library is included in the **Meadow.Foundation.ICs.CAN.Mcp2515** nuget package and is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform.

This driver is part of the [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/) peripherals library, an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT applications.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Installation

You can install the library from within Visual studio using the the NuGet Package Manager or from the command line using the .NET CLI:

`dotnet add package Meadow.Foundation.ICs.CAN.Mcp2515`
## Usage

```csharp
public override Task Initialize()
{
    Resolver.Log.Info("Initialize...");

    expander = new Mcp2515(
        Device.CreateSpiBus(),
        Device.Pins.D05,
        Mcp2515.CanOscillator.Osc_8MHz,
        Device.Pins.D06);

    return base.Initialize();
}

public override async Task Run()
{
    var bus = expander.CreateCanBus(CanBitrate.Can_250kbps);

    Console.WriteLine($"Listening for CAN data...");

    var tick = 0;

    while (true)
    {
        var frame = bus.ReadFrame();
        if (frame != null)
        {
            if (frame is StandardDataFrame sdf)
            {
                Console.WriteLine($"Standard Frame: {sdf.ID:X3} {BitConverter.ToString(sdf.Payload)}");
            }
            else if (frame is ExtendedDataFrame edf)
            {
                Console.WriteLine($"Extended Frame: {edf.ID:X8} {BitConverter.ToString(edf.Payload)}");
            }
        }
        else
        {
            await Task.Delay(100);
        }

        if (tick++ % 50 == 0)
        {
            Console.WriteLine($"Sending Standard Frame...");

            bus.WriteFrame(new StandardDataFrame
            {
                ID = 0x700,
                Payload = new byte[] { 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, (byte)(tick & 0xff) }
            });
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


