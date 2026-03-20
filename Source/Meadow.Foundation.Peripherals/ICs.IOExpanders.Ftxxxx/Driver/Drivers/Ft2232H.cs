using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders;

/// <summary>
/// Represents an FT2232H USB IO expander (high-speed dual-channel with MPSSE support)
/// </summary>
public class Ft2232H : FtdiExpander
{
    internal Ft2232H()
    {
    }

    /// <inheritdoc/>
    public override II2cBus CreateI2cBus(int channel = 0, I2cBusSpeed busSpeed = I2cBusSpeed.Standard)
    {
        // FT2232H supports MPSSE on both channels A and B
        // TODO: Add channel validation and selection
        var bus = new FtMpsseI2cBus(this, busSpeed);
        bus.Configure();
        return bus;
    }

    /// <inheritdoc/>
    public override ISpiBus CreateSpiBus(int channel, SpiClockConfiguration configuration)
    {
        // FT2232H supports MPSSE on both channels A and B
        // TODO: Add channel validation and selection
        var bus = new FtMpsseSpiBus(this, configuration);
        bus.Configure();
        return bus;
    }
}
