using Meadow.Hardware;

namespace Meadow.Foundation.Displays.ePaper
{
    /// <summary>
    /// Represents a WaveShare epd4in2bc ePaper display
    /// 122x250, 2.3inch e-Ink display, SPI interface 
    /// </summary>
    public class Epd2in3 : Il3897
    {
        /// <summary>
        /// Create a new WaveShare Epd2in3 122x250 pixel display object ePaper display object
        /// </summary>
        /// <param name="device">Meadow device</param>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPin">Chip select pin</param>
        /// <param name="dcPin">Data command pin</param>
        /// <param name="resetPin">Reset pin</param>
        /// <param name="busyPin">Busy pin</param>
        public Epd2in3(IMeadowDevice device,
                                    ISpiBus spiBus,
                                    IPin chipSelectPin,
                                    IPin dcPin,
                                    IPin resetPin,
                                    IPin busyPin) :
            base(device, spiBus, chipSelectPin, dcPin, resetPin, busyPin, 122, 250)
        { }

        /// <summary>
        /// Create a new WaveShare Epd2in3 122x250 pixel display object ePaper display object
        /// </summary>
        /// <param name="spiBus">SPI bus connected to display</param>
        /// <param name="chipSelectPort">Chip select output port</param>
        /// <param name="dataCommandPort">Data command output port</param>
        /// <param name="resetPort">Reset output port</param>
        /// <param name="busyPort">Busy input port</param>
        public Epd2in3(ISpiBus spiBus,
                                    IDigitalOutputPort chipSelectPort,
                                    IDigitalOutputPort dataCommandPort,
                                    IDigitalOutputPort resetPort,
                                    IDigitalInputPort busyPort) :
            base(spiBus, chipSelectPort, dataCommandPort, resetPort, busyPort, 122, 250)
        { }
    }
}