using Meadow.Foundation.Sensors.Gnss;
using Meadow.Hardware;

namespace Meadow.Foundation.mikroBUS.Sensors.Gnss
{
    /// <summary>
    /// Represents a mikroBUS GNSS 5 board (Neo M8) 
    /// </summary>
    public class CGNSS5 : NeoM8
    {
        /// <summary>
        /// Creates a new CGNSS5 object using serial
        /// </summary>
        public CGNSS5(IMeadowDevice device, SerialPortName serialPortName, IPin? resetPin, IPin? ppsPin = null)
            : base(device, serialPortName, resetPin, ppsPin)
        { }

        /// <summary>
        /// Creates a new CGNSS5 object using I2C
        /// </summary>
        public CGNSS5(II2cBus i2cBus, IPin? resetPin, IPin? ppsPin = null)
            : base(i2cBus, (byte)Addresses.Default, resetPin: resetPin, ppsPin: ppsPin)
        { }
    }
}