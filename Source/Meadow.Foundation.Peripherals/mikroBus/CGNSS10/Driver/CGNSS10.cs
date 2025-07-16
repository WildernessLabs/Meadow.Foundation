using Meadow.Foundation.Sensors.Gnss;
using Meadow.Hardware;

namespace Meadow.Foundation.mikroBUS.Sensors.Gnss
{
    /// <summary>
    /// Represents a mikroBUS GNSS 10 board (Neo M8) 
    /// </summary>
    public class CGNSS10 : NeoM8
    {
        /// <summary>
        /// Creates a new CGNSS10 object
        /// </summary>
        public CGNSS10(ISpiBus spiBus, IDigitalOutputPort chipSelectPort, IDigitalOutputPort? resetPort = null)
            : base(spiBus, chipSelectPort, resetPort)
        { }

        /// <summary>
        /// Creates a new CGNSS10 object
        /// </summary>
        public CGNSS10(ISpiBus spiBus, IPin chipSelectPin, IPin? resetPin = null)
            : base(spiBus, chipSelectPin, resetPin)
        { }
    }
}