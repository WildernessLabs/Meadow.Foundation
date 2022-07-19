using Meadow.Hardware;
using Meadow.Logging;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public class Sw18AB : SerialWombatBase
    {
        public Sw18AB(II2cBus bus, Address address = SerialWombatBase.Address.Default, Logger? logger = null)
            : base(bus, address, logger)
        {
        }
    }
}