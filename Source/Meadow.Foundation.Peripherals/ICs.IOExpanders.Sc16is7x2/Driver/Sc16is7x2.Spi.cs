using Meadow.Hardware;
using Meadow.Units;

namespace Meadow.Foundation.ICs.IOExpanders
{
    public partial class Sc16is7x2 : ISpiPeripheral
    {
        /// <summary>
        /// The default SPI bus mode for the peripheral
        /// </summary>
        public SpiClockConfiguration.Mode DefaultSpiBusMode => SpiClockConfiguration.Mode.Mode0;
        /// <summary>
        /// The default SPI bus frequency for the peripheral
        /// </summary>
        public Frequency DefaultSpiBusSpeed => throw new System.NotImplementedException();
        /// <summary>
        /// The current SPI bus mode for the peripheral
        /// </summary>
        public SpiClockConfiguration.Mode SpiBusMode { get; set; }
        /// <summary>
        /// The current SPI bus frequency for the peripheral
        /// </summary>
        public Frequency SpiBusSpeed { get; set; }

        private readonly ISpiCommunications? _spiComms;

        /// <summary>
        /// Create a new Sc16is7x2 object
        /// </summary>
        /// <param name="spiBus">The SPI bus connected to the peripheral</param>
        /// <param name="chipSelect">The chip-select port used for the peripheral</param>
        /// <param name="oscillatorFrequency">The frequency of the oscillator connected to the SC16IS</param>
        public Sc16is7x2(ISpiBus spiBus, Frequency oscillatorFrequency, IDigitalOutputPort? chipSelect)
            : this(oscillatorFrequency)
        {
            _spiComms = new SpiCommunications(spiBus, chipSelect, DefaultSpiBusSpeed);
        }
    }
}