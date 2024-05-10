using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Temperature
{
    /// <summary>
    /// Adt7410 Temperature sensor object
    /// </summary>    
    public partial class Adt7410 : ByteCommsSensorBase<Units.Temperature>,
        ITemperatureSensor, II2cPeripheral
    {
        /// <summary>
        /// The default I2C address for the peripheral
        /// </summary>
        public byte DefaultI2cAddress => (byte)Addresses.Default;

        /// <summary>
        /// The temperature from the last reading
        /// </summary>
        public Units.Temperature? Temperature { get; protected set; }

        /// <summary>
        /// Get / set the resolution of the sensor
        /// </summary>
        public Resolution SensorResolution
        {
            get => _sensorResolution;
            set
            {
                BusComms?.ReadRegister((byte)Registers.CONFIG, ReadBuffer.Span);

                if (value == Resolution.Resolution16Bits)
                {
                    ReadBuffer.Span[1] |= 0x80;
                }
                else
                {
                    ReadBuffer.Span[0] &= 0x7F;
                }

                BusComms?.WriteRegister((byte)Registers.CONFIG, ReadBuffer.Span);
                _sensorResolution = value;
            }
        }

        /// <summary>
        /// Backing variable for the SensorResolution property
        /// </summary>
        private Resolution _sensorResolution;

        /// <summary>
        /// Create a new Adt7410 object using the default configuration for the sensor
        /// </summary>
        /// <param name="i2cBus">The I2CBus</param>
        /// <param name="address">I2C address of the sensor</param>
        public Adt7410(II2cBus i2cBus, byte address = (byte)Addresses.Default)
            : base(i2cBus, address, readBufferSize: 2, writeBufferSize: 3)
        {
            BusComms?.ReadRegister(0x01, ReadBuffer.Span);

            _sensorResolution = (ReadBuffer.Span[0] & 0x80) > 0 ?
                     Resolution.Resolution16Bits : Resolution.Resolution13Bits;
        }

        /// <summary>
        /// Update the Temperature property
        /// </summary>
        protected override Task<Units.Temperature> ReadSensor()
        {
            ushort rawValue = BusComms!.ReadRegisterAsUShort((byte)Registers.TEMPMSB, ByteOrder.BigEndian);
            int signedValue;

            if (_sensorResolution == Resolution.Resolution13Bits)
            {
                rawValue >>= 3;
                signedValue = rawValue;

                if ((rawValue & 0x1000) == 0x1000)
                {
                    signedValue -= 8192;
                }
            }
            else
            {
                signedValue = rawValue;
                if ((rawValue & 0x8000) == 0x8000)
                {
                    signedValue -= 65536;
                }
            }

            var degreesPerBit = _sensorResolution == Resolution.Resolution16Bits ? 0.0078125 : 0.0625;

            return Task.FromResult(new Units.Temperature(signedValue * degreesPerBit, Units.Temperature.UnitType.Celsius));
        }
    }
}