using System;
using System.Threading.Tasks;
using Meadow.Hardware;

namespace Meadow.Foundation.ICs.IOExpanders
{
    /// <summary>
    /// Represents a DS3502 digital potentiometer
    /// </summary>
    public partial class Ds3502
    {
        /// <summary>
        /// Default I2C bus speed
        /// </summary>
        public static I2cBusSpeed DefaultBusSpeed => I2cBusSpeed.Fast;

        private readonly II2cPeripheral i2cPeripheral;

        /// <summary>
        /// Create a new Ds3502 object using the default parameters
        /// </summary>
        /// <param name="address">Address of the bus on the I2C display</param>
        /// <param name="i2cBus">I2C bus instance</param>
        public Ds3502(II2cBus i2cBus, byte address = (byte)Addresses.Default)
        {
            i2cPeripheral = new I2cPeripheral(i2cBus, address);

            i2cPeripheral.WriteRegister((byte)Register.DS3502_MODE, 0x80);
        }

        /// <summary>
        /// Get the current wiper value 
        /// </summary>
        /// <returns>the 7-bit wiper value</returns>
        public byte GetWiper()
        {
            return i2cPeripheral.ReadRegister((byte)Register.DS3502_WIPER);
        }

        /// <summary>
        /// Set the wiper value
        /// </summary>
        /// <param name="value">wiper value (0-127)</param>
        public void SetWiper(byte value)
        {
            value = Math.Min(value, (byte)127);

            i2cPeripheral.WriteRegister((byte)Register.DS3502_WIPER, value);
        }

        /// <summary>
        /// Set the default wiper value
        /// </summary>
        /// <param name="value">wiper value (0-127)</param>
        public async Task SetWiperDefault(byte value)
        {
            value = Math.Min(value, (byte)127);
            
            i2cPeripheral.WriteRegister((byte)Register.DS3502_MODE, 0x00);
            i2cPeripheral.WriteRegister((byte)Register.DS3502_WIPER, value);

            await Task.Delay(100).ConfigureAwait(false);

            i2cPeripheral.WriteRegister((byte)Register.DS3502_MODE, 0x80);
        }
    }
}