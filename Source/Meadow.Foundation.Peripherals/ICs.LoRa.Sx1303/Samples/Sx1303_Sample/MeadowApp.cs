using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.LoRa;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Sx1303_Sample;

// Target: Meadow.Linux on Raspberry Pi with WaveShare SX1303 915M Gateway (B) HAT
//
// WaveShare HAT default GPIO assignments (BCM numbering):
//   SPI0 CLK  : GPIO 11  (pin 23)
//   SPI0 MOSI : GPIO 10  (pin 19)
//   SPI0 MISO : GPIO  9  (pin 21)
//   SPI0 CE0  : GPIO  8  (pin 24)  ← chip select
//   Reset     : GPIO 23  (pin 16)
//   Power En  : GPIO 18  (pin 12)
//
// Firmware: download sx1302_hal from https://github.com/Lora-net/sx1302_hal
//   and build it.  The compiled binaries are in:
//     sx1302_hal/bin/chip_id             (for testing)
//   and the firmware byte arrays are in:
//     sx1302_hal/libloragw/inc/loragw_agc_params.h
//     sx1302_hal/libloragw/inc/loragw_arb_params.h
//
//   For convenience, the compiled packet forwarder writes firmware to:
//     /tmp/sx1302_agc_fw.bin
//     /tmp/sx1302_arb_fw.bin
//   when run with the --fwdump flag.  Alternatively, build the HAL and
//   extract the arrays from the header files.

public class RaspberryPiApp : App<RaspberryPi>
{
    private Sx1303? _gateway;

    //<!=SNIP=>

    public override Task Initialize()
    {
        Resolver.Log.Info("Initializing WaveShare SX1303 915M LoRaWAN Gateway...");

        // Create SPI bus — Mode 0 (CPOL=0, CPHA=0), up to 8 MHz
        var spiBus = Device.CreateSpiBus(
            clock: Device.Pins.SPI5CLK,
            mosi:  Device.Pins.SPI5MOSI,
            miso:  Device.Pins.SPI5MISO,
            config: new Meadow.Hardware.SpiClockConfiguration(
                Sx1303.DefaultSpiClockSpeed,
                Sx1303.SpiMode));

        _gateway = new Sx1303(
            spiBus:          spiBus,
            chipSelectPin:   Device.Pins.GPIO8,   // SPI0 CE0
            resetPin:        Device.Pins.GPIO23,  // WaveShare reset
            powerEnablePin:  Device.Pins.GPIO18,  // WaveShare power enable
            logger:          Resolver.Log);

        // Load firmware from files extracted from the Semtech sx1302_hal SDK.
        // See comments at the top of this file for how to obtain these.
        var agcFw = File.ReadAllBytes("/home/pi/sx1302_hal/firmware/agc_fw.bin");
        var arbFw = File.ReadAllBytes("/home/pi/sx1302_hal/firmware/arb_fw.bin");

        _gateway.Initialize(agcFw, arbFw, ChannelPlan.US915_SubBand1);

        _gateway.PacketReceived += OnPacketReceived;

        return base.Initialize();
    }

    public override Task Run()
    {
        Resolver.Log.Info("Starting LoRaWAN concentrator — listening on US915 sub-band 1...");

        _gateway!.Start(pollIntervalMs: 10);

        // Keep the app alive; packets arrive via the event handler
        return Task.Delay(-1);
    }

    private void OnPacketReceived(object? sender, LoRaRxPacket packet)
    {
        Resolver.Log.Info($"[RX] {packet}");
        Resolver.Log.Info($"     Payload ({packet.Payload.Length} bytes): " +
                          BitConverter.ToString(packet.Payload));
    }

    //<!=SNOP=>
}
