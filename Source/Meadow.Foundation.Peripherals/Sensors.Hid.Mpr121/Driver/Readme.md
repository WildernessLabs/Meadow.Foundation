# Meadow.Foundation.Sensors.Hid.Mpr121

**Freescale Semiconductor MPR121 I2C capacitive keypad controller**

The **Mpr121** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/)

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/), to view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/)

## Usage

```
public override Task Initialize()
{
    Resolver.Log.Info("Initialize...");

    var sensor = new Mpr121(Device.CreateI2cBus(Meadow.Hardware.I2cBusSpeed.Standard), 90, 100);
    sensor.ChannelStatusesChanged += Sensor_ChannelStatusesChanged;

    return Task.CompletedTask;
}

private void Sensor_ChannelStatusesChanged(object sender, ChannelStatusChangedEventArgs e)
{
    string pads = string.Empty;

    for(int i = 0; i < e.ChannelStatus.Count; i++)
    {
        if(e.ChannelStatus[(Mpr121.Channels)i] == true)
        {
            pads += i + ", ";
        }
    }

    var msg = string.IsNullOrEmpty(pads) ? "none" : (pads + "touched");
    Resolver.Log.Info(msg);
}

```
