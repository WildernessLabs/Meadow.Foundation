using System;
using Meadow.Foundation.ICs.IOExpanders.Ports;

namespace Meadow.Foundation.ICs.IOExpanders
{
    internal class Mcp23x17RegisterMap : IMcp23RegisterMap
    {
        private const ushort PortA = 0;
        private const ushort PortB = 1;

        // Using lazy to create a singleton safely
        private static readonly Lazy<Mcp23x17RegisterMap> _instance =
            new Lazy<Mcp23x17RegisterMap>(() => new Mcp23x17RegisterMap());

        private readonly Mcp23PortRegisterMap _portAPairedMap = new Mcp23PortRegisterMap(
            new (byte address, Mcp23PortRegister register)[]
            {
                (0x00, Mcp23PortRegister.IODirectionRegister),
                (0x02, Mcp23PortRegister.InputPolarityRegister),
                (0x04, Mcp23PortRegister.InterruptOnChangeRegister),
                (0x06, Mcp23PortRegister.DefaultComparisonValueRegister),
                (0x08, Mcp23PortRegister.InterruptControlRegister),
                (0x0A, Mcp23PortRegister.IOConfigurationRegister),
                (0x0C, Mcp23PortRegister.PullupResistorConfigurationRegister),
                (0x0E, Mcp23PortRegister.InterruptFlagRegister),
                (0x10, Mcp23PortRegister.InterruptCaptureRegister),
                (0x12, Mcp23PortRegister.GPIORegister),
                (0x14, Mcp23PortRegister.OutputLatchRegister)
            });

        private readonly Mcp23PortRegisterMap _portASegregatedMap = new Mcp23PortRegisterMap(
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

        private readonly Mcp23PortRegisterMap _portBPairedMap = new Mcp23PortRegisterMap(
            new (byte address, Mcp23PortRegister register)[]
            {
                (0x01, Mcp23PortRegister.IODirectionRegister),
                (0x03, Mcp23PortRegister.InputPolarityRegister),
                (0x05, Mcp23PortRegister.InterruptOnChangeRegister),
                (0x07, Mcp23PortRegister.DefaultComparisonValueRegister),
                (0x09, Mcp23PortRegister.InterruptControlRegister),
                (0x0B, Mcp23PortRegister.IOConfigurationRegister),
                (0x0D, Mcp23PortRegister.PullupResistorConfigurationRegister),
                (0x0F, Mcp23PortRegister.InterruptFlagRegister),
                (0x11, Mcp23PortRegister.InterruptCaptureRegister),
                (0x13, Mcp23PortRegister.GPIORegister),
                (0x15, Mcp23PortRegister.OutputLatchRegister)
            });

        private readonly Mcp23PortRegisterMap _portBSegregatedMap = new Mcp23PortRegisterMap(
            new (byte address, Mcp23PortRegister register)[]
            {
                (0x10, Mcp23PortRegister.IODirectionRegister),
                (0x11, Mcp23PortRegister.InputPolarityRegister),
                (0x12, Mcp23PortRegister.InterruptOnChangeRegister),
                (0x13, Mcp23PortRegister.DefaultComparisonValueRegister),
                (0x14, Mcp23PortRegister.InterruptControlRegister),
                (0x15, Mcp23PortRegister.IOConfigurationRegister),
                (0x16, Mcp23PortRegister.PullupResistorConfigurationRegister),
                (0x17, Mcp23PortRegister.InterruptFlagRegister),
                (0x18, Mcp23PortRegister.InterruptCaptureRegister),
                (0x19, Mcp23PortRegister.GPIORegister),
                (0x1A, Mcp23PortRegister.OutputLatchRegister)
            });

        private Mcp23x17RegisterMap()
        {
        }

        public static Mcp23x17RegisterMap Instance => _instance.Value;

        public byte GetAddress(Mcp23PortRegister register, int port, BankConfiguration bank)
        {
            return GetMap(port, bank).GetAddress(register);
        }

        public Mcp23PortRegister GetNextRegister(byte currentAddress, int port, BankConfiguration bank)
        {
            return GetMap(port, bank).GetNextRegister(currentAddress);
        }

        public Mcp23PortRegister GetRegisterAtAddress(byte address, int port, BankConfiguration bank)
        {
            return GetMap(port, bank).GetRegisterAtAddress(address);
        }

        private Mcp23PortRegisterMap GetMap(int port, BankConfiguration bank)
        {
            switch (port)
            {
                case PortA:
                    switch (bank)
                    {
                        case BankConfiguration.Paired:
                            return _portAPairedMap;
                        case BankConfiguration.Segregated:
                            return _portASegregatedMap;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(bank), bank, "Bank must be either Paired or Segregated");
                    }

                case PortB:
                    switch (bank)
                    {
                        case BankConfiguration.Paired:
                            return _portBPairedMap;
                        case BankConfiguration.Segregated:
                            return _portBSegregatedMap;
                        default:
                            throw new ArgumentOutOfRangeException(nameof(bank), bank, "Bank must be either Paired or Segregated");
                    }

                default:
                    throw new ArgumentOutOfRangeException(nameof(port), port, "Port must be either 0 or 1");
            }
        }
    }
}
