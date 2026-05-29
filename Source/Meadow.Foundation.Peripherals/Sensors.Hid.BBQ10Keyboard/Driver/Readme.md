# Meadow.Foundation.Sensors.Hid.Bbq10Keyboard

**BB Q10 I2C Keyboard**

The **Bbq10Keyboard** library is included in the **Meadow.Foundation.Sensors.Hid.Bbq10Keyboard** nuget package and is designed for the [Wilderness Labs](https://www.wildernesslabs.co) Meadow .NET IoT platform.

This driver is part of the [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/) peripherals library, an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT applications.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Installation

You can install the library from within Visual Studio using the NuGet Package Manager or from the command line using the .NET CLI:

`dotnet add package Meadow.Foundation.Sensors.Hid.Bbq10Keyboard`
## Usage

```csharp
public override Task Initialize()
{
    Resolver.Log.Info("Initialize...");

    var i2cBus = Device.CreateI2cBus(0);

    // Interrupt-driven mode: pass the interrupt pin and key events
    // fire automatically via the OnKeyEvent event.
    keyboard = new BBQ10Keyboard(i2cBus, Device.Pins.D10);

    // Without an interrupt pin, use polling instead:
    // keyboard = new BBQ10Keyboard(i2cBus);
    // keyboard.StartPolling(intervalMs: 50);

    keyboard.OnKeyEvent += Keyboard_OnKeyEvent;

    return Task.CompletedTask;
}

private void Keyboard_OnKeyEvent(object? sender, BBQ10Keyboard.KeyEvent e)
{
    if (e.KeyState == BBQ10Keyboard.KeyState.StatePress)
    {
        Resolver.Log.Info($"Key pressed: '{e.AsciiValue}' (0x{(byte)e.AsciiValue:X2})");
    }
    else if (e.KeyState == BBQ10Keyboard.KeyState.StateRelease)
    {
        Resolver.Log.Info($"Key released: '{e.AsciiValue}'");
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

Meadow is a complete IoT platform with defense-grade security that runs full .NET applications on embeddable microcontrollers and Linux single-board computers including Raspberry Pi and NVIDIA Jetson.

### Build

Use the full .NET platform and tooling such as Visual Studio and plug-and-play hardware drivers to painlessly build IoT solutions.

### Connect

Utilize native support for WiFi, Ethernet, and Cellular connectivity to send sensor data to the Cloud and remotely control your peripherals.

### Deploy

Instantly deploy and manage your fleet in the cloud for OTA, health-monitoring, logs, command + control, and enterprise backend integrations.


