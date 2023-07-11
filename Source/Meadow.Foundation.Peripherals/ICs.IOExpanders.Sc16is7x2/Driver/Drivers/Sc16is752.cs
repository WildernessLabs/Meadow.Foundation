using Meadow.Hardware;
using Meadow.Units;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public class Sc16is752 : Sc16is7x2
    {
        public Sc16is752(Frequency oscillatorFrequency, IDigitalInterruptPort? irq) : base(oscillatorFrequency, irq)
        {
        }

        public Sc16is752(II2cBus i2cBus, Frequency oscillatorFrequency, Addresses address = Addresses.Address_0x48, IDigitalInterruptPort? irq = null) : base(i2cBus, oscillatorFrequency, address, irq)
        {
        }

        public Sc16is752(ISpiBus spiBus, Frequency oscillatorFrequency, IDigitalOutputPort? chipSelect = null, IDigitalInterruptPort? irq = null) : base(spiBus, oscillatorFrequency, chipSelect, irq)
        {
        }
    }
}
