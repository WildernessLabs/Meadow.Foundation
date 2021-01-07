using System;
using System.Collections.Generic;

namespace Meadow.Foundation.ICs.IOExpanders.Ports
{
    public readonly struct Mcp23PortRegisterMap
    {
        /// number of items in
        /// <see cref="Mcp23PortRegister" />
        private const int RegisterSize = 12;

        // The index of this array maps to Mcp23PortRegister
        private readonly byte[] _mappingArray;

        /// <summary>
        /// Helper struct to map register types to addresses.
        /// </summary>
        /// <param name="mapping"></param>
        /// <remarks>
        /// Known limitation: If not all registers are provided, any register not provided will specify their address as 0x00.
        /// </remarks>
        public Mcp23PortRegisterMap(IEnumerable<(byte address, Mcp23PortRegister register)> mapping)
        {
            if (mapping == null)
            {
                throw new ArgumentNullException(nameof(mapping));
            }

            _mappingArray = new byte[RegisterSize];

            foreach (var (address, register) in mapping)
            {
                // Skip invalid, all addresses are invalid unless specified otherwise
                if (register == Mcp23PortRegister.Invalid)
                {
                    continue;
                }

                if (_mappingArray[(int) register] > 0)
                {
                    throw new ArgumentException(
                        $"The same register '{register}' cannot be mapped to multiple addresses.",
                        nameof(mapping));
                }

                _mappingArray[(int) register] = address;
            }
        }

        public byte GetAddress(Mcp23PortRegister register)
        {
            if (register == Mcp23PortRegister.Invalid || (int) register < 0 || (int) register > RegisterSize - 1)
            {
                throw new ArgumentOutOfRangeException(nameof(register), register, "Must be valid register");
            }

            return _mappingArray[(int) register];
        }

        public Mcp23PortRegister GetRegisterAtAddress(byte address)
        {
            // _mappingArray[0] maps to Mcp23PortRegister.Invalid which should never be checked.
            for (var i = 1; i < _mappingArray.Length; i++)
            {
                if (_mappingArray[i] == address)
                {
                    return (Mcp23PortRegister) i;
                }
            }

            return Mcp23PortRegister.Invalid;
        }

        public Mcp23PortRegister GetNextRegister(byte currentAddress)
        {
            if (currentAddress == byte.MaxValue)
            {
                return Mcp23PortRegister.Invalid;
            }

            currentAddress += 1;

            return GetRegisterAtAddress(currentAddress);
        }
    }
}
