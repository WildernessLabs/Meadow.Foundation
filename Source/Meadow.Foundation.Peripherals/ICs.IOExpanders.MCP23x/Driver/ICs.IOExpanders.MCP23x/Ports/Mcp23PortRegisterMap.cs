using System;
using System.Collections.Generic;

namespace Meadow.Foundation.ICs.IOExpanders.Ports
{
    public readonly struct Mcp23PortRegisterMap
    {
        /// number of items in
        /// <see cref="Mcp23PortRegister" />
        private const int RegisterSize = 12;

        private readonly byte[] _mappingArray;

        public Mcp23PortRegisterMap(IEnumerable<(byte address, Mcp23PortRegister register)> mapping)
        {
            _mappingArray = new byte[RegisterSize];

            var hasInvalid = false;
            foreach (var (address, register) in mapping)
            {
                _mappingArray[(int) register] = address;
                hasInvalid = hasInvalid || register == Mcp23PortRegister.Invalid;
            }

        }

        public byte GetAddress(Mcp23PortRegister register)
        {
            if (register == Mcp23PortRegister.Invalid)
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
                if (_mappingArray[0] == address)
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
