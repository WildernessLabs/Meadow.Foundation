namespace Meadow.Foundation.Sensors.Atmospheric
{
    internal abstract class BME280Comms
    {
        /// <summary>
        ///     Registers used to control the BME280.
        /// </summary>
        internal enum Register : byte
        {
            ChipID = 0xd0,
            Reset = 0xe0,
            Humidity = 0xf2,
            Status = 0xf3,
            Measurement = 0xf4,
            Configuration = 0xf5,
        }

        public abstract void WriteRegister(Register register, byte value);
        public abstract byte[] ReadRegisters(byte startRegister, int readCount);
    }
}