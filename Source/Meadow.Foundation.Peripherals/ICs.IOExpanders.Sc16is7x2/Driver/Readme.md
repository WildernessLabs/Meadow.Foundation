# Meadow.Foundation.ICs.IOExpanders.Sc16is7x2

**SC16IS7x2 SPI I2C family of UART expanders (SC16IS752, SC16IS762)**

The **Sc16is7x2** library is included in the **Meadow.Foundation.ICs.IOExpanders.Sc16is7x2** nuget package and is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform.

This driver is part of the [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/) peripherals library, an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT applications.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Installation

You can install the library from within Visual studio using the the NuGet Package Manager or from the command line using the .NET CLI:

`dotnet add package Meadow.Foundation.ICs.IOExpanders.Sc16is7x2`
## Usage

```csharp
private Sc16is752? _expander = null;

private ISerialPort? _expanderPort = null;
private ISerialPort? _nativePort = null;

private const int Rate = 9600;

public override async Task Initialize()
{
    // NOTE: This sample connects PortA on the expander back to COM1 on the F7Feather
    Resolver.Log.Info("Initialize...");

    var address = Sc16is7x2.Addresses.Address_0x4D;

    try
    {
        _expander = new Sc16is752(
            Device.CreateI2cBus(),
            new Meadow.Units.Frequency(1.8432, Meadow.Units.Frequency.UnitType.Megahertz),
            address);

        _expanderPort = _expander.PortA.CreateSerialPort(Rate);
        _nativePort = Device.PlatformOS.GetSerialPortName("com1")?.CreateSerialPort(Rate);
    }
    catch (Exception ex)
    {
        Resolver.Log.Error($"Failed to initialize 0x{(byte)address:X2}: {ex.Message}");
        await Task.Delay(1000);
    }
}

public override Task Run()
{
    if (_expander == null || _expanderPort == null || _nativePort == null)
    {
        return Task.CompletedTask;
    }

    Task.Run(() => PingReply(_expanderPort, _nativePort));

    while (true)
    {
        Thread.Sleep(1000);
    }
}

private async Task PingReply(ISerialPort portA, ISerialPort portB)
{
    var readBuffer = new byte[64];

    Resolver.Log.Info("Opening ports...");
    portA.Open();
    portB.Open();

    var abMessage = "ping\n";
    var baMessage = "reply\n";
    var count = 0;

    while (true)
    {
        try
        {
            // A -> B
            Resolver.Log.Info($"TEST A->B");
            var messageAB = Encoding.ASCII.GetBytes(abMessage);
            Resolver.Log.Info($"Writing {messageAB.Length} bytes on A...");
            Resolver.Log.Info($"  {BitConverter.ToString(messageAB)}");

            portA.Write(messageAB);

            Resolver.Log.Info($"Reading on B...");
            count = 0;
            while (count < messageAB.Length)
            {
                count += portB.Read(readBuffer, count, messageAB.Length);
            }

            Resolver.Log.Info($"Read {count} bytes on B");
            Resolver.Log.Info($"  {BitConverter.ToString(readBuffer, 0, count)}");

            var rxString = Encoding.ASCII.GetString(readBuffer, 0, count);
            Resolver.Log.Info($"A sent: {abMessage}");
            Resolver.Log.Info($"B received: {rxString}\n");

            // B -> A
            Resolver.Log.Info($"TEST B->A");
            var messageBA = Encoding.ASCII.GetBytes(baMessage);
            Resolver.Log.Info($"Responding with {messageBA.Length} bytes on B...");
            Resolver.Log.Info($"  {BitConverter.ToString(messageBA)}");

            portB.Write(messageBA);
            Resolver.Log.Info($"Reading on A...");
            count = 0;
            while (count < messageBA.Length)
            {
                count += portA.Read(readBuffer, count, messageBA.Length);
            }

            Resolver.Log.Info($"Read {count} bytes on A");
            Resolver.Log.Info($"  {BitConverter.ToString(readBuffer, 0, count)}");

            rxString = Encoding.ASCII.GetString(readBuffer, 0, count);
            Resolver.Log.Info($"B sent: {baMessage}");
            Resolver.Log.Info($"A received: {rxString}\n");

            await Task.Delay(30000);
        }
        catch (Exception ex)
        {
            Resolver.Log.Error($"Test error: {ex.Message}");
            await Task.Delay(5000);
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


