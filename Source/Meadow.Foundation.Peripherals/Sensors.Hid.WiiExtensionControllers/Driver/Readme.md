# Meadow.Foundation.Sensors.Hid.WiiExtensionControllers

**Nintendo Wii I2C extension controllers (nunchuck, classic controller, snes classic controller, nes classic controller)**

The **WiiExtensionControllers** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/)

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/), to view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/)

## Usage

```csharp
NesClassicController nesController;

public override Task Initialize()
{
    Resolver.Log.Info("Initialize...");

    var i2cBus = Device.CreateI2cBus(NesClassicController.DefaultI2cSpeed);

    nesController = new NesClassicController(i2cBus: i2cBus);

    //onetime update - could be used in a game loop
    nesController.Update();

    //check the state of a button
    Resolver.Log.Info("X Button is " + (nesController.AButton.State == true ? "pressed" : "not pressed"));

    //.NET events
    nesController.AButton.Clicked += (s, e) => Resolver.Log.Info("A button clicked");
    nesController.BButton.Clicked += (s, e) => Resolver.Log.Info("B button clicked");

    nesController.StartButton.Clicked += (s, e) => Resolver.Log.Info("+ button clicked");
    nesController.SelectButton.Clicked += (s, e) => Resolver.Log.Info("- button clicked");

    nesController.DPad.Updated += (s, e) => Resolver.Log.Info($"DPad {e.New}");

    return Task.CompletedTask;
}

public override Task Run()
{
    nesController.StartUpdating(TimeSpan.FromMilliseconds(200));
    return Task.CompletedTask;
}

```
