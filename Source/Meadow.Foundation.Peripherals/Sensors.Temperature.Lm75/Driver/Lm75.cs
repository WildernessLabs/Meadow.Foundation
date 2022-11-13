using Meadow.Hardware;
using Meadow.Peripherals.Sensors;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.Sensors.Temperature
{
    /// <summary>
    /// TMP102 Temperature sensor object
    /// </summary>    
    public partial class Lm75 : ByteCommsSensorBase<Units.Temperature>, ITemperatureSensor
    {
        /// <summary>
        /// Raised when the value of the reading changes
        /// </summary>
        public event EventHandler<IChangeResult<Units.Temperature>> TemperatureUpdated = delegate { };

        /// <summary>
        /// The Temperature value from the last reading
        /// </summary>
        public Units.Temperature? Temperature { get; protected set; }

        /// <summary>
        /// Create a new TMP102 object using the default configuration for the sensor
        /// </summary>
        /// <param name="i2cBus">The I2C bus</param>
        /// <param name="address">I2C address of the sensor</param>
        public Lm75(II2cBus i2cBus, byte address = (byte)Addresses.Default)
            : base(i2cBus, address)
        {
        }

        /// <summary>
        /// Update the Temperature property
        /// </summary>
        protected override Task<Units.Temperature> ReadSensor()
        {
            return Task.Run(() =>
            {
                Peripheral?.Write((byte)Registers.LM_TEMP);

                Peripheral?.ReadRegister((byte)Registers.LM_TEMP, ReadBuffer.Span[0..2]);

                // Details in Datasheet P10
                double temp = 0;
                ushort raw = (ushort)((ReadBuffer.Span[0] << 3) | (ReadBuffer.Span[1] >> 5));
                if ((ReadBuffer.Span[0] & 0x80) == 0)
                {
                    // temperature >= 0
                    temp = raw * 0.125;
                }
                else
                {
                    raw |= 0xF800;
                    raw = (ushort)(~raw + 1);

                    temp = raw * (-1) * 0.125;
                }

                //only accurate to +/- 0.1 degrees
                return (new Units.Temperature((float)Math.Round(temp, 1), Units.Temperature.UnitType.Celsius));

            });
        }

        /// <summary>
        /// Raise events for subcribers and notify of value changes
        /// </summary>
        /// <param name="changeResult">The updated sensor data</param>
        protected override void RaiseEventsAndNotify(IChangeResult<Units.Temperature> changeResult)
        {
            TemperatureUpdated?.Invoke(this, changeResult);
            base.RaiseEventsAndNotify(changeResult);
        }
    }
}