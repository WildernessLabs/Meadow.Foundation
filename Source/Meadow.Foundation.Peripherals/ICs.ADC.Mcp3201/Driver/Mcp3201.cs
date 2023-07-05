using Meadow.Hardware;
using Meadow.Units;

namespace Meadow.Foundation.ICs.ADC
{
    public abstract partial class Mcp3201 : PollingSensorBase<Voltage>, ISpiPeripheral
    {
        public SpiClockConfiguration.Mode DefaultSpiBusMode => throw new System.NotImplementedException();

        public SpiClockConfiguration.Mode SpiBusMode { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }

        public Frequency DefaultSpiBusSpeed => throw new System.NotImplementedException();

        public Frequency SpiBusSpeed { get => throw new System.NotImplementedException(); set => throw new System.NotImplementedException(); }
    }
}