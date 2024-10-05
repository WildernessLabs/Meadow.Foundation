using FTD2XX;
using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders;

/// <summary>
/// Represents an FT232H USB IO expander
/// </summary>
public class Ft232h : FtdiExpander
{
    private readonly FTDI _ftdiDevice = new();

    internal Ft232h()
    {
    }

    /// <inheritdoc/>
    public override II2cBus CreateI2cBus(int channel = 0, I2cBusSpeed busSpeed = I2cBusSpeed.Standard)
    {
        // TODO: depends on part
        // TODO: make sure no SPI is in use
        var bus = new Ft232hI2cBus(_ftdiDevice, channel, busSpeed);
        return bus;
    }

    /// <inheritdoc/>
    public override ISpiBus CreateSpiBus(int channel, SpiClockConfiguration configuration)
    {
        // TODO: make sure no SPI is in use
        var bus = new Ft232hSpiBus(this, configuration);
        bus.Configure();
        return bus;
    }
}