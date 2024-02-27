using Meadow.Hardware;
using System;

namespace Meadow.Foundation.ICs.IOExpanders;

public class Ft2232Expander : FtdiExpander
{
    internal Ft2232Expander()
    {
    }

    /// <inheritdoc/>
    public override II2cBus CreateI2cBus(int channel = 0, I2cBusSpeed busSpeed = I2cBusSpeed.Standard)
    {
        // TODO: depends on part
        // TODO: make sure no SPI is in use
        var bus = new Ft23xxxI2cBus(this, busSpeed);
        bus.Configure();
        return bus;
    }

    /// <inheritdoc/>
    public override ISpiBus CreateSpiBus(int channel, SpiClockConfiguration configuration)
    {
        // TODO: make sure no SPI is in use

        throw new NotSupportedException();
    }
}
