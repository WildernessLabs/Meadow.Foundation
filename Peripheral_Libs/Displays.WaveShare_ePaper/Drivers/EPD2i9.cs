using Meadow.Hardware;

namespace Meadow.Foundation.Displays.Drivers
{
    public class EPD2i9 : EPD1i54
    {
        public EPD2i9(Pins chipSelectPin, Pins dcPin, Pins resetPin, Pins busyPin,
            SPI.SPI_module spiModule = SPI.SPI_module.SPI1, uint speedKHz = (uint)9500):base(chipSelectPin, dcPin, resetPin, busyPin, spiModule, speedKHz)
        { }

        public override uint Width => 128;
        public override uint Height => 296;
    }
}