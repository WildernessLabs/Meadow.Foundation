using System;
using Meadow.Foundation.ICs.IOExpanders.Ports;

namespace Meadow.Foundation.ICs.IOExpanders
{
    internal class Mcp23x08RegisterMap : IMcp23RegisterMap
    {
        // Using lazy to create a singleton safely
        private static readonly Lazy<Mcp23x08RegisterMap> _instance =
            new Lazy<Mcp23x08RegisterMap>(() => new Mcp23x08RegisterMap());

        private readonly Mcp23PortRegisterMap _portMap = new Mcp23PortRegisterMap(
            new (byte address, Mcp23PortRegister register)[]
            {
                (0x00, Mcp23PortRegister.IODirectionRegister),
                (0x01, Mcp23PortRegister.InputPolarityRegister),
                (0x02, Mcp23PortRegister.InterruptOnChangeRegister),
                (0x03, Mcp23PortRegister.DefaultComparisonValueRegister),
                (0x04, Mcp23PortRegister.InterruptControlRegister),
                (0x05, Mcp23PortRegister.IOConfigurationRegister),
                (0x06, Mcp23PortRegister.PullupResistorConfigurationRegister),
                (0x07, Mcp23PortRegister.InterruptFlagRegister),
                (0x08, Mcp23PortRegister.InterruptCaptureRegister),
                (0x09, Mcp23PortRegister.GPIORegister),
                (0x0A, Mcp23PortRegister.OutputLatchRegister)
            });

        private Mcp23x08RegisterMap()
        {
        }

        public static Mcp23x08RegisterMap Instance => _instance.Value;

        public byte GetAddress(Mcp23PortRegister register, int port, BankConfiguration bank)
        {
            if (port != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(port));
            }
            
            return _portMap.GetAddress(register);
        }

        public Mcp23PortRegister GetNextRegister(byte currentAddress, int port, BankConfiguration bank)
        {
            if (port != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(port));
            }

            return _portMap.GetRegisterAtAddress(currentAddress);
        }

        public Mcp23PortRegister GetRegisterAtAddress(byte address, int port, BankConfiguration bank)
        {
            if (port != 0)
            {
                throw new ArgumentOutOfRangeException(nameof(port));
            }

            return _portMap.GetRegisterAtAddress(address);
        }
    }
}
