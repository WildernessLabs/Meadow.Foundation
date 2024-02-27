# Meadow.Foundation.ICs.EEPROM.At24Cxx

**At24Cxx I2C EEPROMs (AT24C32 / AT24C64 / AT24C128 / AT24C256)**

The **At24Cxx** library is included in the **Meadow.Foundation.ICs.EEPROM.At24Cxx** nuget package and is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform.

This driver is part of the [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/) peripherals library, an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT applications.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Installation

You can install the library from within Visual studio using the the NuGet Package Manager or from the command line using the .NET CLI:

`dotnet add package Meadow.Foundation.ICs.EEPROM.At24Cxx`
## Usage

```csharp
At24Cxx eeprom;

public override Task Initialize()
{
    Resolver.Log.Info("Initialize...");

    //256kbit = 256*1024 bits = 262144 bits = 262144 / 8 bytes = 32768 bytes
    //if you're using the ZS-042 board, it has an AT24C32 and uses the default value of 8192
    eeprom = new At24Cxx(i2cBus: Device.CreateI2cBus(), memorySize: 32768);

    return base.Initialize();
}

public override Task Run()
{
    Resolver.Log.Info("Write to eeprom");
    eeprom.Write(0, new byte[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 });

    Resolver.Log.Info("Read from eeprom");
    var memory = eeprom.Read(0, 16);

    for (ushort index = 0; index < 16; index++)
    {
        Thread.Sleep(50);
        Resolver.Log.Info("Byte: " + index + ", Value: " + memory[index]);
    }

    eeprom.Write(3, new byte[] { 10 });
    eeprom.Write(7, new byte[] { 1, 2, 3, 4 });
    memory = eeprom.Read(0, 16);

    for (ushort index = 0; index < 16; index++)
    {
        Thread.Sleep(50);
        Resolver.Log.Info("Byte: " + index + ", Value: " + memory[index]);
    }

    return base.Run();
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


