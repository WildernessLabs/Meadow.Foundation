# Meadow.Foundation.Leds.Pca9633

**Pca9633 I2C RGB LED driver**

The **Pca9633** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/)

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/), to view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/)

## Usage

```
Pca9633 pca9633;

public override Task Initialize()
{
    Resolver.Log.Info("Initialize...");

    pca9633 = new Pca9633(Device.CreateI2cBus());

    return base.Initialize();
}

public override Task Run()
{
    //set the location of R,G,B leds for color control
    pca9633.SetRgbLedPositions(redLed: Pca9633.LedPosition.Led2,
                              greenLed: Pca9633.LedPosition.Led1,
                              blueLed: Pca9633.LedPosition.Led0);

    //set a single color
    pca9633.SetColor(Color.Red);
    Thread.Sleep(1000);
    pca9633.SetColor(Color.Blue);
    Thread.Sleep(1000);
    pca9633.SetColor(Color.Yellow);

    return base.Run();
}

```
