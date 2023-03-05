using Meadow.Hardware;

namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Represents a WaveShare Epd1in54c tri color ePaper color display
    /// 152x152, 1.54" e-Ink three-color display, SPI interface 
    /// </summary>
    public class Epd1in54c : Il0376F
    {
        /// <summary>
        /// Create a new WaveShare Epd1in54c 152x152 pixel display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPin">Chip select pin</param>
        /// <param name="dcPin">Data command pin</param>
        /// <param name="resetPin">Reset pin</param>
        /// <param name="busyPin">Busy pin</param>
        public Epd1in54c(ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin, IPin busyPin) :
            base(spiBus, chipSelectPin, dcPin, resetPin, busyPin, 152, 152)
        { }

        /// <summary>
        /// Create a new WaveShare Epd1in54c ePaper 152x152 pixel display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPort">Chip select output port</param>
        /// <param name="dataCommandPort">Data command output port</param>
        /// <param name="resetPort">Reset output port</param>
        /// <param name="busyPort">Busy input port</param>
        public Epd1in54c(ISpiBus spiBus,
            IDigitalOutputPort chipSelectPort,
            IDigitalOutputPort dataCommandPort,
            IDigitalOutputPort resetPort,
            IDigitalInputPort busyPort) :
            base(spiBus, chipSelectPort, dataCommandPort, resetPort, busyPort, 152, 152)
        { }
    }
}