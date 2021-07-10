using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using Meadow.Units;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Temperature
{
    /// <summary>
    /// TMP102 Temperature sensor object.
    /// </summary>    
    public partial class Lm75 : ByteCommsSensorBase<Units.Temperature>, ITemperatureSensor
    {
        //==== Events
        /// <summary>
        /// Raised when the value of the reading changes.
        /// </summary>
        public event EventHandler<IChangeResult<Units.Temperature>> TemperatureUpdated = delegate { };

        //==== internals

        //==== properties
        public byte DEFAULT_ADDRESS => 0x48;

        /// <summary>
        /// The Temperature value from the last reading.
        /// </summary>
        public Units.Temperature? Temperature { get; protected set; }

        /// <summary>
        ///     Create a new TMP102 object using the default configuration for the sensor.
        /// </summary>
        /// <param name="address">I2C address of the sensor.</param>
        public Lm75(II2cBus i2cBus, byte address = 0x48)
            : base(i2cBus, address)
        {
        }

        /// <summary>
        /// Update the Temperature property.
        /// </summary>
        protected override Task<Units.Temperature> ReadSensor()
        {
            return Task.Run(() => {

                Peripheral.Write((byte)Registers.LM_TEMP);

                Peripheral.ReadRegister((byte)Registers.LM_TEMP, ReadBuffer.Span[0..2]);

                // Details in Datasheet P10
                double temp = 0;
                ushort raw = (ushort)((ReadBuffer.Span[0] << 3) | (ReadBuffer.Span[1] >> 5));
                if ((ReadBuffer.Span[0] & 0x80) == 0) {
                    // temperature >= 0
                    temp = raw * 0.125;
                } else {
                    raw |= 0xF800;
                    raw = (ushort)(~raw + 1);

                    temp = raw * (-1) * 0.125;
                }

                //only accurate to +/- 0.1 degrees
                return(new Units.Temperature((float)Math.Round(temp, 1), Units.Temperature.UnitType.Celsius));

            });
        }

        protected override void RaiseEventsAndNotify(IChangeResult<Units.Temperature> changeResult)
        {
            TemperatureUpdated?.Invoke(this, changeResult);
            base.RaiseEventsAndNotify(changeResult);
        }
    }
}