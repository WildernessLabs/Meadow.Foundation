# Meadow.Foundation.Sensors.Radio.Rfid.IDxxLA

**IDxxLA Serial radio frequency ID readers**

The **IDxxLA** library is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform and is part of [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/).

The **Meadow.Foundation** peripherals library is an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT application.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Usage

```csharp
IRfidReader rfidReader;

public override Task Initialize()
{
    Resolver.Log.Info("Initialize...");

    rfidReader = new IDxxLA(Device, Device.PlatformOS.GetSerialPortName("COM1"));

    // subscribe to event
    rfidReader.RfidRead += RfidReaderOnTagRead;

    // subscribe to IObservable
    rfidReader.Subscribe(new RfidObserver());

    return Task.CompletedTask;
}

public override Task Run()
{ 
    rfidReader.StartReading();

    return Task.CompletedTask;
}

private void RfidReaderOnTagRead(object sender, RfidReadResult e)
{
    if (e.Status == RfidValidationStatus.Ok) {
        Resolver.Log.Info($"From event - Tag value is {DebugInformation.Hexadecimal(e.RfidTag)}");
        return;
    }

    Resolver.Log.Error($"From event - Error {e.Status}");
}

private class RfidObserver : IObserver<byte[]>
{
    public void OnCompleted()
    {
        Resolver.Log.Info("From IObserver - RfidReader has terminated, no more events will be emitted.");
    }
     
    public void OnError(Exception error)
    {
        Resolver.Log.Error($"From IObserver - {error}");
    }

    public void OnNext(byte[] value)
    {
        Resolver.Log.Info($"From IObserver - Tag value is {DebugInformation.Hexadecimal(value)}");
    }
}

```
## How to Contribute

- **Found a bug?** [Report an issue](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Have a **feature idea or driver request?** [Open a new feature request](https://github.com/WildernessLabs/Meadow_Issues/issues)
- Want to **contribute code?** Fork the [Meadow.Foundation](https://github.com/WildernessLabs/Meadow.Foundation) repository and submit a pull request against the `develop` branch


## Need Help?

If you have questions or need assistance, please join the Wilderness Labs [community on Slack](http://slackinvite.wildernesslabs.co/).
