using Meadow.Hardware;
using System;

namespace Meadow.Foundation.ICs.IOExpanders;

/// <summary>
/// Represents an FT4232 USB IO expander
/// </summary>
public class Ft4232 : FtdiExpander
{
    internal Ft4232()
    {
    }

    /// <inheritdoc/>
    public override II2cBus CreateI2cBus(int channel = 0, I2cBusSpeed busSpeed = I2cBusSpeed.Standard)
    {
        throw new NotSupportedException();
    }

    /// <inheritdoc/>
    public override ISpiBus CreateSpiBus(int channel, SpiClockConfiguration configuration)
    {
        throw new NotSupportedException();
    }
}