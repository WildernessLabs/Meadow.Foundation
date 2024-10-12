using Meadow.Hardware;

namespace Meadow.Foundation.ICs.DAC
{
    public abstract partial class Ad569x : II2cPeripheral
    {
        public enum Addresses
        {
            Address_0C = 0b0001100,
            Address_0D = 0b0001101,
            Address_0E = 0b0001110,
            Address_0F = 0b0001111,
            Default = Address_0C,
        }

        internal Ad569x(II2cBus bus)
        {
        }
    }
}