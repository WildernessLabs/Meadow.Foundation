# Meadow.Foundation.ICs.EEPROM.At24Cxx

**At24Cxx I2C EEPROMs (AT24C32 / AT24C64 / AT24C128 / AT24C256)**

The **At24Cxx** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/)

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/), to view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/)

## Usage

```
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
