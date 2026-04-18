# Meadow.Foundation.Transceivers.SeeedWioE5

**Seeed Wio-E5 LoRa/LoRaWAN transceiver module**

The **WioE5** library is included in the **Meadow.Foundation.Transceivers.SeeedWioE5** nuget package and is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform.

This driver is part of the [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/) peripherals library, an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT applications.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## Installation

You can install the library from within Visual studio using the the NuGet Package Manager or from the command line using the .NET CLI:

`dotnet add package Meadow.Foundation.Transceivers.SeeedWioE5`

## Usage

```csharp
WioE5 radio;

public override async Task Initialize()
{
    Resolver.Log.Info("Initialize...");

    // Grove UART connector on Project Lab V1: com1 (RX=D13, TX=D12)
    var portName = Device.PlatformOS.GetSerialPortName("com1")!;
    radio = new WioE5(portName);

    radio.PacketReceived += (s, pkt) =>
    {
        var text = System.Text.Encoding.ASCII.GetString(pkt.Payload);
        Resolver.Log.Info($"RX: \"{text}\" RSSI:{pkt.Rssi}dBm SNR:{pkt.Snr}dB");
    };

    var ok = await radio.Initialize();
    Resolver.Log.Info($"Initialize: {(ok ? "OK" : "FAILED")}");
}

public override async Task Run()
{
    await radio.SetTestMode();
    await radio.ConfigureRf(frequencyHz: 868_000_000);

    await radio.SendPacket(System.Text.Encoding.ASCII.GetBytes("Hello from Meadow"));
    await radio.StartReceiving();
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


