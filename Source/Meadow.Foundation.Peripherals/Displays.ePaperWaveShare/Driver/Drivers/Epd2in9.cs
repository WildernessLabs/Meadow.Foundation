using Meadow.Hardware;

namespace Meadow.Foundation.Displays
{
    /// <summary>
    /// Represents a WaveShare Epd2in9 ePaper display
    /// 128x296, 2.9 inch e-Ink display, SPI interface 
    /// </summary>
    public class Epd2in9 : Ssd1608
    {
        /// <summary>
        /// Create a new WaveShare Epd2in9 128x296 pixel display object ePaper display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPin">Chip select pin</param>
        /// <param name="dcPin">Data command pin</param>
        /// <param name="resetPin">Reset pin</param>
        /// <param name="busyPin">Busy pin</param>
        public Epd2in9(ISpiBus spiBus,
                       IPin chipSelectPin,
                       IPin dcPin,
                       IPin resetPin,
                       IPin busyPin) :
            base(spiBus, chipSelectPin, dcPin, resetPin, busyPin, 128, 296)
        { }

        /// <summary>
        /// Create a new WaveShare Epd2in9 128x296 pixel display object ePaper display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPort">Chip select output port</param>
        /// <param name="dataCommandPort">Data command output port</param>
        /// <param name="resetPort">Reset output port</param>
        /// <param name="busyPort">Busy input port</param>
        public Epd2in9(ISpiBus spiBus,
                        IDigitalOutputPort chipSelectPort,
                        IDigitalOutputPort dataCommandPort,
                        IDigitalOutputPort resetPort,
                        IDigitalInputPort busyPort) :
            base(spiBus, chipSelectPort, dataCommandPort, resetPort, busyPort, 128, 296)
        { }
    }
}