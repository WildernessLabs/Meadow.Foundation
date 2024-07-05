# Meadow.Foundation.ICs.IOExpanders.As1115

**AS1115 I2C IO expander, led driver, character display controller and keyscan**

The **As1115** library is included in the **Meadow.Foundation.ICs.IOExpanders.As1115** nuget package and is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform.

This driver is part of the [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/) peripherals library, an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT applications.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Installation

You can install the library from within Visual studio using the the NuGet Package Manager or from the command line using the .NET CLI:

`dotnet add package Meadow.Foundation.ICs.IOExpanders.As1115`
## Usage

```csharp
As1115 as1115;
MicroGraphics graphics;

public override Task Initialize()
{
    Resolver.Log.Info("Initialize...");
    as1115 = new As1115(Device.CreateI2cBus(), Device.Pins.D03);

    //general key scan events - will raise for all buttons
    as1115.KeyScanPressStarted += KeyScanPressStarted;

    //or access buttons as IButtons individually
    as1115.KeyScanButtons[KeyScanButtonType.Button1].LongClickedThreshold = TimeSpan.FromSeconds(1);
    as1115.KeyScanButtons[KeyScanButtonType.Button1].Clicked += Button1_Clicked;
    as1115.KeyScanButtons[KeyScanButtonType.Button1].LongClicked += Button1_LongClicked; ;

    graphics = new MicroGraphics(as1115);

    return base.Initialize();
}

private void Button1_LongClicked(object sender, EventArgs e)
{
    Resolver.Log.Info("Button 1 long press");
}

private void Button1_Clicked(object sender, EventArgs e)
{
    Resolver.Log.Info("Button 1 clicked");
}

private void KeyScanPressStarted(object sender, KeyScanEventArgs e)
{
    Resolver.Log.Info($"{e.Button} pressed");
}

public override Task Run()
{
    graphics.Clear();
    graphics.DrawLine(0, 0, 7, 7, true);
    graphics.DrawLine(0, 7, 7, 0, true);

    graphics.Show();

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


