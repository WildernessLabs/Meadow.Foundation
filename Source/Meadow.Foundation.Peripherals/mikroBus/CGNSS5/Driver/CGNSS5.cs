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
        /// Creates a new CGNSS5 object using a MikroBus connector (serial)
        /// </summary>
        /// <param name="connector">The MikroBus connector</param>
        public CGNSS5(MikroBusConnector connector)
            : base(connector.SerialPortName, connector.Pins.RST)
        { }

        /// <summary>
        /// Creates a new CGNSS5 object using serial
        /// </summary>
        public CGNSS5(SerialPortName serialPortName, IPin? resetPin = null, IPin? ppsPin = null)
            : base(serialPortName, resetPin, ppsPin)
        { }

        /// <summary>
        /// Creates a new CGNSS5 object using I2C
        /// </summary>
        public CGNSS5(II2cBus i2cBus, IPin? resetPin = null, IPin? ppsPin = null)
            : base(i2cBus, (byte)Addresses.Default, resetPin: resetPin, ppsPin: ppsPin)
        { }
    }
}