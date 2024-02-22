using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders;

public class Ft232hExpander : FtdiExpander
{
    internal Ft232hExpander()
    {
    }

    /// <inheritdoc/>
    public override II2cBus CreateI2cBus(int channel = 0, I2cBusSpeed busSpeed = I2cBusSpeed.Standard)
    {
        // TODO: depends on part
        // TODO: make sure no SPI is in use
        var bus = new Ft232hI2cBus(this, busSpeed);
        bus.Configure();
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
