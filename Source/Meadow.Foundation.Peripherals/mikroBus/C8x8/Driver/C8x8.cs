using Meadow.Foundation.Displays;
using Meadow.Hardware;

namespace Meadow.Foundation.mikroBUS.Displays
{
    /// <summary>
    /// Represents a mikroBUS 8x8 Click board
    /// </summary>
    public class C8x8 : Max7219
    {
        /// <summary>
        /// Creates a new MikroBus 8x8 object 
        /// </summary>
        /// <param name="spiBus">SPI bus</param>
        /// <param name="chipselectPort">Chip select port</param>
        public C8x8(ISpiBus spiBus, IDigitalOutputPort chipselectPort) 
            : base(spiBus, chipselectPort, 1, Max7219Mode.Display)
        { }

        /// <summary>
        /// Creates a new MikroBus 8x8 object
        /// </summary>
        /// <param name="spiBus">SPI bus</param>
        /// <param name="chipSelectPin">Chip select pin</param>
        public C8x8(ISpiBus spiBus, IPin chipSelectPin)
            : base(spiBus, chipSelectPin, 1, Max7219Mode.Display)
        { }

        /// <summary>
        /// Creates a new MikroBus 8x8 object using a MikroBus connector
        /// </summary>
        /// <param name="connector">The MikroBus connector</param>
        public C8x8(MikroBusConnector connector)
            : base(connector.SpiBus, connector.Pins.CS, 1, Max7219Mode.Display)
        { }
    }
}