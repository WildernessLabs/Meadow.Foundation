# Meadow.Foundation.ICs.LoRa.Sx1303

**Semtech SX1303 LoRaWAN multi-channel concentrator (WaveShare SX1303 915M Gateway B)**

The **Sx1303** library is included in the **Meadow.Foundation.ICs.LoRa.Sx1303** nuget package and is designed for the [Wilderness Labs](www.wildernesslabs.co) Meadow .NET IoT platform.

This driver is part of the [Meadow.Foundation](https://developer.wildernesslabs.co/Meadow/Meadow.Foundation/) peripherals library, an open-source repository of drivers and libraries that streamline and simplify adding hardware to your C# .NET Meadow IoT applications.

For more information on developing for Meadow, visit [developer.wildernesslabs.co](http://developer.wildernesslabs.co/).

To view all Wilderness Labs open-source projects, including samples, visit [github.com/wildernesslabs](https://github.com/wildernesslabs/).

## About the Hardware

The WaveShare SX1303 915M LoRaWAN Gateway (B) is a Raspberry Pi HAT based on the Semtech SX1303 multi-channel LoRaWAN concentrator chip, paired with two SX1250 RF front-ends.

Key capabilities:
- 8 simultaneous multi-SF (SF5–SF12) uplink channels, 125/250/500 kHz bandwidth
- 1 single-SF (STD) LoRa channel
- 1 FSK channel
- US915 / AU915 / EU868 / AS923 / IN865 regional band support
- SPI interface, up to 8 MHz, Mode 0 (CPOL=0, CPHA=0)

## Hardware Connections (Raspberry Pi, BCM GPIO numbering)

| Signal        | RPi GPIO | Physical Pin |
|---------------|----------|--------------|
| SPI0 SCLK     | GPIO 11  | Pin 23       |
| SPI0 MOSI     | GPIO 10  | Pin 19       |
| SPI0 MISO     | GPIO  9  | Pin 21       |
| SPI0 CE0 (CS) | GPIO  8  | Pin 24       |
| SX1303 Reset  | GPIO 23  | Pin 16       |
| Power Enable  | GPIO 18  | Pin 12       |

## Firmware Requirement

The SX1303 contains two internal MCUs (AGC and ARB) that require firmware to operate.  These firmware binaries are distributed as part of the Semtech **sx1302_hal** open-source SDK:

> https://github.com/Lora-net/sx1302_hal

Build the HAL and extract the firmware byte arrays from:
- `libloragw/inc/loragw_agc_params.h` → `agc_fw.bin`
- `libloragw/inc/loragw_arb_params.h` → `arb_fw.bin`

Each file must be exactly **8192 bytes**.

## Installation

```
dotnet add package Meadow.Foundation.ICs.LoRa.Sx1303
```

## Usage

```csharp
public override Task Initialize()
{
    Resolver.Log.Info("Initializing SX1303 gateway...");

    var spiBus = Device.CreateSpiBus(
        clock: Device.Pins.SPI5CLK,
        mosi:  Device.Pins.SPI5MOSI,
        miso:  Device.Pins.SPI5MISO,
        config: new SpiClockConfiguration(
            Sx1303.DefaultSpiClockSpeed,
            Sx1303.SpiMode));

    var gateway = new Sx1303(
        spiBus:         spiBus,
        chipSelectPin:  Device.Pins.GPIO8,
        resetPin:       Device.Pins.GPIO23,
        powerEnablePin: Device.Pins.GPIO18,
        logger:         Resolver.Log);

    var agcFw = File.ReadAllBytes("/home/pi/sx1302_hal/firmware/agc_fw.bin");
    var arbFw = File.ReadAllBytes("/home/pi/sx1302_hal/firmware/arb_fw.bin");

    gateway.Initialize(agcFw, arbFw, ChannelPlan.US915_SubBand1);

    gateway.PacketReceived += (s, packet) =>
    {
        Resolver.Log.Info($"Received: {packet}");
        Resolver.Log.Info($"Payload: {BitConverter.ToString(packet.Payload)}");
    };

    gateway.Start();

    return base.Initialize();
}
```

## Channel Plans

Pre-built channel plans are provided as static properties on `ChannelPlan`:

| Property                    | Band   | Description                            |
|-----------------------------|--------|----------------------------------------|
| `ChannelPlan.US915_SubBand1`| US915  | Channels 0-7 + 1 BW500 uplink          |
| `ChannelPlan.EU868`         | EU868  | Standard 8-channel EU plan             |

You can also build a custom plan:

```csharp
var plan = new ChannelPlan
{
    RadioAFrequencyHz = 916_800_000,
    RadioBFrequencyHz = 917_600_000,
    MultiSfChains = new IfChainConfig[8]
    {
        new() { Radio = Sx1303.RadioPath.RadioA, FrequencyOffsetHz = -400_000, Enabled = true },
        // ... etc.
    }
};
```

## Transmitting (Downlink)

```csharp
gateway.SendPacket(new LoRaTxPacket
{
    FrequencyHz     = 923_300_000,   // 923.3 MHz
    SpreadingFactor = Sx1303.SpreadingFactor.SF10,
    Bandwidth       = Sx1303.Bandwidth.BW125,
    CodingRate      = Sx1303.CodingRate.CR4_5,
    Power           = Sx1303.TxPower.Medium,
    InvertIq        = true,          // required for LoRaWAN downlinks
    Payload         = new byte[] { 0x60, 0x00, 0x00, 0x00, 0x00 },
});
```

## Register Address Note

SX1303 register addresses in this driver are derived from the Semtech
**sx1302_hal** open-source HAL (`libloragw/inc/loragw_reg.h`).  If a
register address appears incorrect for your specific HAL version, refer
to that file's `loregs[]` table for the authoritative physical address.

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
