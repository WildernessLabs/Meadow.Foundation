using Meadow.Hardware;
using System;
using System.Threading.Tasks;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Represents a DS3502 digital potentiometer
    /// </summary>
    public partial class Ds3502 : II2cPeripheral
    {
        /// <summary>
        /// The default I2C address for the peripheral
        /// </summary>
        public byte DefaultI2cAddress => (byte)Addresses.Default;

        /// <summary>
        /// Default I2C bus speed
        /// </summary>
        public static I2cBusSpeed DefaultBusSpeed => I2cBusSpeed.Fast;

        /// <summary>
        /// I2C Communication bus used to communicate with the peripheral
        /// </summary>
        protected readonly II2cCommunications i2cComms;

        /// <summary>
        /// Create a new Ds3502 object using the default parameters
        /// </summary>
        /// <param name="address">Address of the bus on the I2C display</param>
        /// <param name="i2cBus">I2C bus instance</param>
        public Ds3502(II2cBus i2cBus, byte address = (byte)Addresses.Default)
        {
            i2cComms = new I2cCommunications(i2cBus, address);

            i2cComms.WriteRegister((byte)Register.DS3502_MODE, 0x80);
        }

        /// <summary>
        /// Get the current wiper value 
        /// </summary>
        /// <returns>the 7-bit wiper value</returns>
        public byte GetWiper()
        {
            return i2cComms.ReadRegister((byte)Register.DS3502_WIPER);
        }

        /// <summary>
        /// Set the wiper value
        /// </summary>
        /// <param name="value">wiper value (0-127)</param>
        public void SetWiper(byte value)
        {
            value = Math.Min(value, (byte)127);

            i2cComms.WriteRegister((byte)Register.DS3502_WIPER, value);
        }

        /// <summary>
        /// Set the default wiper value
        /// </summary>
        /// <param name="value">wiper value (0-127)</param>
        public async Task SetWiperDefault(byte value)
        {
            value = Math.Min(value, (byte)127);

            i2cComms.WriteRegister((byte)Register.DS3502_MODE, 0x00);
            i2cComms.WriteRegister((byte)Register.DS3502_WIPER, value);

            await Task.Delay(100).ConfigureAwait(false);

            i2cComms.WriteRegister((byte)Register.DS3502_MODE, 0x80);
        }
    }
}