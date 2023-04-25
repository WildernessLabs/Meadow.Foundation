using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Temperature
{
    /// <summary>
    /// TMP102 Temperature sensor object
    /// </summary>    
    public partial class Tmp102 : ByteCommsSensorBase<Units.Temperature>,
        ITemperatureSensor, II2cPeripheral
    {
        /// <summary>
        /// The default I2C address for the peripheral
        /// </summary>
        public byte I2cDefaultAddress => (byte)Address.Default;

        /// <summary>
        /// Raised when the temperature value changes
        /// </summary>
        public event EventHandler<IChangeResult<Units.Temperature>> TemperatureUpdated = delegate { };

        /// <summary>
        /// Backing variable for the SensorResolution property
        /// </summary>
        private Resolution _sensorResolution;

        /// <summary>
        /// Get / set the resolution of the sensor
        /// </summary>
        public Resolution SensorResolution
        {
            get => _sensorResolution;
            set
            {
                BusComms?.ReadRegister(0x01, ReadBuffer.Span);

                if (value == Resolution.Resolution12Bits)
                {
                    ReadBuffer.Span[1] &= 0xef;
                }
                else
                {
                    ReadBuffer.Span[1] |= 0x10;
                }

                BusComms?.WriteRegister(0x01, ReadBuffer.Span);
                _sensorResolution = value;
            }
        }

        /// <summary>
        /// The temperature from the last reading
        /// </summary>
        public Units.Temperature? Temperature { get; protected set; }

        /// <summary>
        /// Create a new TMP102 object using the default configuration for the sensor
        /// </summary>
        /// <param name="i2cBus">The I2CBus</param>
        /// <param name="address">I2C address of the sensor</param>
        public Tmp102(II2cBus i2cBus, byte address = (byte)Address.Default)
            : base(i2cBus, address, readBufferSize: 2, writeBufferSize: 2)
        {
            BusComms?.ReadRegister(0x01, ReadBuffer.Span);
            _sensorResolution = (ReadBuffer.Span[1] & 0x10) > 0 ?
                                 Resolution.Resolution13Bits : Resolution.Resolution12Bits;
        }

        /// <summary>
        /// Update the Temperature property
        /// </summary>
        protected override Task<Units.Temperature> ReadSensor()
        {
            BusComms?.ReadRegister(0x00, ReadBuffer.Span);

            int sensorReading;
            if (SensorResolution == Resolution.Resolution12Bits)
            {
                sensorReading = (ReadBuffer.Span[0] << 4) | (ReadBuffer.Span[1] >> 4);
            }
            else
            {
                sensorReading = (ReadBuffer.Span[0] << 5) | (ReadBuffer.Span[1] >> 3);
            }

            return Task.FromResult(new Units.Temperature((float)(sensorReading * 0.0625), Units.Temperature.UnitType.Celsius));
        }

        /// <summary>
        /// Raise change events for subscribers
        /// </summary>
        /// <param name="changeResult">The change result with the current sensor data</param>
        protected void RaiseChangedAndNotify(IChangeResult<Units.Temperature> changeResult)
        {
            TemperatureUpdated?.Invoke(this, changeResult);
            base.RaiseEventsAndNotify(changeResult);
        }
    }
}