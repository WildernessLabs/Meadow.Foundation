using Meadow.Hardware;
using System;

namespace Meadow.Foundation.Transceivers
{
    /// <summary>
    /// Represents the Semech SX127x Low Power Long Range Transceivers 
    /// </summary>
    public class SX127x : SpiPeripheral
    {
        private const byte REG_VERSION = 0x42;

        /// <summary>
        /// Creates a new SX127x object
        /// </summary>
        /// <param name="i2cbus">I2C bus</param>
        /// <param name="chipSelectPort">The port for the chip select pin</param>
        public SX127x(ISpiBus i2cbus, IDigitalOutputPort chipSelectPort) 
            : base(i2cbus, chipSelectPort)
        { }

        /// <summary>
        /// Get the hardware version
        /// </summary>
        /// <returns>The version as a byte</returns>
        public byte GetVersion()
        {
            try
            {
                return ReadRegister(REG_VERSION);
            }
            catch (Exception ex)
            {
                Console.WriteLine("ReadDeviceID " + ex.Message);
                return 0xff;
            }
        }
    }
}