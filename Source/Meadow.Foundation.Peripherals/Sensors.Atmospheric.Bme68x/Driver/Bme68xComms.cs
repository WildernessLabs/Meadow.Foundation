using System;

namespace Meadow.Foundation.Sensors.Atmospheric
{
    internal abstract class Bme68xComms
    {
        public abstract void WriteRegister(byte address, byte value);
        
        public abstract void ReadRegister(byte address, Span<byte> readBuffer);

        public abstract byte ReadRegister(byte address);

        public abstract ushort ReadRegisterAsUShort(byte address, ByteOrder order = ByteOrder.LittleEndian);
    }
}