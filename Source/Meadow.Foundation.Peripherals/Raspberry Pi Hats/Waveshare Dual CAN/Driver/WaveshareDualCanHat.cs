using Meadow.Foundation.ICs.CAN;
using Meadow.Hardware;
using Meadow.Units;
using System;
using static Meadow.Foundation.ICs.CAN.Mcp2515;

namespace Meadow.Foundation;

/// <summary>
/// Represents a Waveshare Dual CAN Hat for Raspberry Pi
/// </summary>
public class WaveshareDualCanHat
{
    private const int DefaultSpiBusSpeed = 5_000_000; // 5MHz

    /// <summary>
    /// The default CAN bitrate (500kbps)
    /// </summary>
    public const CanBitrate DefaultCanBitrate = CanBitrate.Can_500kbps;

    private Lazy<Mcp2515> _mcp0;
    private Lazy<Mcp2515> _mcp1;

    private Lazy<ICanBus> _can0;
    private Lazy<ICanBus> _can1;

    // INT0 -> pin16
    // MOSI -> pin19
    // MISO -> pin21
    // INT1 -> pin22
    // SCK -> pin23
    // CS0 -> pin24
    // CS1 -> pin26

    /// <summary>
    /// The first CAN bus (CAN0)
    /// </summary>
    public ICanBus CAN0 => _can0.Value;

    /// <summary>
    /// The second CAN bus (CAN1)
    /// </summary>
    public ICanBus CAN1 => _can1.Value;

    /// <summary>
    /// Creates a new WaveshareDualCanHat instance
    /// </summary>
    /// <param name="device">The Raspberry Pi device</param>
    /// <param name="can0Rate">The bitrate for CAN0 (default is 500kbps)</param>
    /// <param name="can1Rate">The bitrate for CAN1 (default is 500kbps)</param>
    public WaveshareDualCanHat(
        Meadow.RaspberryPi device,
        CanBitrate can0Rate = DefaultCanBitrate,
        CanBitrate can1Rate = DefaultCanBitrate)
    {
        var spi0 = device.CreateSpiBus(0, DefaultSpiBusSpeed.Hertz());

        _mcp0 = new Lazy<Mcp2515>(() => new Mcp2515(spi0, device.Pins.Pin24, CanOscillator.Osc_16MHz, device.Pins.Pin16));
        _mcp1 = new Lazy<Mcp2515>(() => new Mcp2515(spi0, device.Pins.Pin26, CanOscillator.Osc_16MHz, device.Pins.Pin22));

        _can0 = new Lazy<ICanBus>(() =>
        {
            try
            {
                Resolver.Log.Info("Creating CAN0 bus...");
                var bus = _mcp0.Value.CreateCanBus(can0Rate, 0);
                Resolver.Log.Info("CAN0 bus created");
                return bus;
            }
            catch (NativeException)
            {
                Resolver.Log.Error("Failed to initialize CAN0 - the SPI CS is probably in use.  Did you update the boot.ini with a no-chip-select overlay? (dtoverlay=spi0-0cs)");
                throw;
            }
        });
        _can1 = new Lazy<ICanBus>(() =>
        {
            try
            {
                Resolver.Log.Info("Creating CAN1 bus...");
                var bus = _mcp1.Value.CreateCanBus(can1Rate, 0);
                Resolver.Log.Info("CAN1 bus created");
                return bus;
            }
            catch (NativeException)
            {
                Resolver.Log.Error("Failed to initialize CAN1 - the SPI CS is probably in use.  Did you update the boot.ini with a no-chip-select overlay? (dtoverlay=spi0-0cs)");
                throw;
            }
        });
    }
}
