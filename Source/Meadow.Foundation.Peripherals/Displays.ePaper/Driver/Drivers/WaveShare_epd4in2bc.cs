using Meadow.Hardware;

namespace Meadow.Foundation.Displays.ePaper
{
    /// <summary>
    /// Represents a WaveShare epd4in2bc tri color 4.2" ePaper color display (red or yellow)
    /// 400x300, 4.2inch e-Ink three-color display, SPI interface 
    /// </summary>
    public class WaveShare_epd4in2bc : Il0398
    {
        /// <summary>
        /// Create a new WaveShare_epd4in2bc 400x300 pixel display object ePaper display object
        /// </summary>
        /// <param name="device">Meadow device</param>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPin">Chip select pin</param>
        /// <param name="dcPin">Data command pin</param>
        /// <param name="resetPin">Reset pin</param>
        /// <param name="busyPin">Busy pin</param>
        public WaveShare_epd4in2bc(IMeadowDevice device, 
                                    ISpiBus spiBus, 
                                    IPin chipSelectPin, 
                                    IPin dcPin, 
                                    IPin resetPin, 
                                    IPin busyPin) :
            base(device, spiBus, chipSelectPin, dcPin, resetPin, busyPin)
        { }

        /// <summary>
        /// Create a new WaveShare_epd4in2bc 400x300 pixel display object ePaper display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPort">Chip select output port</param>
        /// <param name="dataCommandPort">Data command output port</param>
        /// <param name="resetPort">Reset output port</param>
        /// <param name="busyPort">Busy input port</param>
        public WaveShare_epd4in2bc(ISpiBus spiBus,
                                    IDigitalOutputPort chipSelectPort,
                                    IDigitalOutputPort dataCommandPort,
                                    IDigitalOutputPort resetPort,
                                    IDigitalInputPort busyPort) :
            base(spiBus, chipSelectPort, dataCommandPort, resetPort, busyPort)
        { }
    }
}