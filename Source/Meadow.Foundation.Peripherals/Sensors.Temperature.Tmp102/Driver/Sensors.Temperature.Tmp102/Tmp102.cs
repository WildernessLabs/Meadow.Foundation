using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Peripherals.Sensors;

namespace Meadow.Foundation.Sensors.Temperature
{
    // TODO: BC: this peripheral hasn't been tested since it got updated to the
    // new hotness. I don't have one, but I will order one.
    /// <summary>
    /// TMP102 Temperature sensor object.
    /// </summary>    
    public partial class Tmp102 : ByteCommsSensorBase<Units.Temperature>, ITemperatureSensor
    {
        //==== events
        public event EventHandler<IChangeResult<Units.Temperature>> TemperatureUpdated = delegate { };

        //==== properties
        /// <summary>
        /// Backing variable for the SensorResolution property.
        /// </summary>
        private Resolution _sensorResolution;

        /// <summary>
        ///     Get / set the resolution of the sensor.
        /// </summary>
        public Resolution SensorResolution 
        {
            get => _sensorResolution; 
            set 
            {
                Peripheral.ReadRegister(0x01, ReadBuffer.Span);
                // TODO: Delete after testing
                //var configuration = Peripheral.ReadRegisters(0x01, 2);
                if (value == Resolution.Resolution12Bits) 
                {
                    ReadBuffer.Span[1] &= 0xef;
                } else {
                    ReadBuffer.Span[1] |= 0x10;
                }
                // @CTACKE: is there a better way here? do we need a WriteRegisters that takes a Span<byte>?
                Peripheral.WriteRegisters(0x01, ReadBuffer.Span.ToArray());
                _sensorResolution = value;
            }
        }

        /// <summary>
        /// The temperature from the last reading.
        /// </summary>
        public Units.Temperature? Temperature { get; protected set; }

        /// <summary>
        ///     Create a new TMP102 object using the default configuration for the sensor.
        /// </summary>
        /// <param name="address">I2C address of the sensor.</param>
        public Tmp102(II2cBus i2cBus, byte address = 0x48)
            : base(i2cBus, address, readBufferSize: 2, writeBufferSize: 2)
        {
            // TODO: Delete after testing
            //var configuration = Peripheral.ReadRegisters(0x01, 2);
            //_sensorResolution = (configuration[1] & 0x10) > 0 ?
            //                     Resolution.Resolution13Bits : Resolution.Resolution12Bits;
            Peripheral.ReadRegister(0x01, ReadBuffer.Span);
            _sensorResolution = (ReadBuffer.Span[1] & 0x10) > 0 ?
                                 Resolution.Resolution13Bits : Resolution.Resolution12Bits;
        }

        /// <summary>
        /// Update the Temperature property.
        /// </summary>
        protected override Task<Units.Temperature> ReadSensor()
        {
            Peripheral.ReadRegister(0x00, ReadBuffer.Span);
            // TODO: Delete after testing
            //var temperatureData = Peripheral.ReadRegisters(0x00, 2);

            var sensorReading = 0;
            if (SensorResolution == Resolution.Resolution12Bits) {
                sensorReading = (ReadBuffer.Span[0] << 4) | (ReadBuffer.Span[1] >> 4);
            } else {
                sensorReading = (ReadBuffer.Span[0] << 5) | (ReadBuffer.Span[1] >> 3);
            }

            return Task.FromResult(new Units.Temperature((float)(sensorReading * 0.0625), Units.Temperature.UnitType.Celsius));
        }

        protected void RaiseChangedAndNotify(IChangeResult<Units.Temperature> changeResult)
        {
            TemperatureUpdated?.Invoke(this, changeResult);
            base.RaiseEventsAndNotify(changeResult);
        }
    }
}