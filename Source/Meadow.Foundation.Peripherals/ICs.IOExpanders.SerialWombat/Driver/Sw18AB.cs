using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public class Sw18AB : SerialWombatBase
    {
        public Sw18AB(II2cBus bus, Address address = SerialWombatBase.Address.Default)
            : base(bus, address)
        {
        }
    }
}