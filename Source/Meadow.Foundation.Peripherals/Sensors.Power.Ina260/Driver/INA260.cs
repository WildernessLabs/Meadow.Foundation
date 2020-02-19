using Meadow.Hardware;
using System;

namespace Meadow.Foundation.Sensors.Power
{
    public class INA260 : IDisposable
    {
        /// <summary>
        ///     Valid addresses for the sensor.
        /// </summary>
        public enum Addresses : byte
        {
            Address0 = 0x40,
            Address1 = 0x41,
            Default = Address0
        }

        private enum Register : byte
        {
            Config = 0x00,
            Current = 0x01,
            Voltage = 0x02,
            Power = 0x03,
            MaskEnable = 0x06,
            AlertLimit = 0x07,
            ManufacturerID = 0xFE,
            DieID = 0xFF
        }

        private const float MeasurementScale = 0.00125f;

        private Register RegisterPointer { get; set; }
        private object SyncRoot { get; } = new object();
        private II2cBus Bus { get; set; }

        public byte Address { get; private set; }

        public INA260(II2cBus i2cBus, byte address = (byte)Addresses.Default)
        {
            if (i2cBus == null) throw new ArgumentNullException(nameof(i2cBus));

            switch (address)
            {
                case 0x40:
                case 0x41:
                    // valid;
                    break;
                default:
                    throw new ArgumentOutOfRangeException("INA200 device address must be either 0x40 or 0x41");
            }

            Bus = i2cBus;
            Address = address;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                
            }
        }

        /// <summary>
        /// Dispose managed resources
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
        }

        /// <summary>
        /// Reads the unique manufacturer identification number
        /// </summary>
        public int ManufacturerID
        {
            get => ReadRegister(Register.ManufacturerID);
        }

        /// <summary>
        /// Reads the unique die identification number
        /// </summary>
        public int DieID
        {
            get => ReadRegister(Register.ManufacturerID);
        }

        /// <summary>
        /// Reads the value of the current (in Amps) flowing through the shunt resistor
        /// </summary>
        public float Current
        {
            get
            {
                unchecked
                {
                    var c = (short)ReadRegister(Register.Current);
                    return c * MeasurementScale;
                }
            }
        }

        /// <summary>
        /// Reads bus voltage measurement (in Volts) data
        /// </summary>
        public float Voltage
        {
            get => ReadRegister(Register.Voltage) * MeasurementScale;
        }

        /// <summary>
        /// Reads the value of the calculated power being delivered to the load
        /// </summary>
        public float Power
        {
            get => ReadRegister(Register.Power);
        }

        private ushort ReadRegister(Register register)
        {
            lock (SyncRoot)
            {
                Span<byte> buffer = stackalloc byte[2];

                if (register != RegisterPointer)
                {
                    // write the pointer
                    buffer[0] = (byte)register;
                    Bus.WriteData(Address, buffer[0]);

                    RegisterPointer = register;
                }

                var data = Bus.ReadData(Address, 2);

                return (ushort)((data[0] << 8) | data[1]);
            }
        }
    }
}
