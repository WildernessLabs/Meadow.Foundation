using Meadow.Hardware;

namespace Meadow.Foundation.Displays.ePaper
{
    public class EPD2i9 : EPD1i54
    {
        public EPD2i9(IIODevice device, ISpiBus spiBus, IPin chipSelectPin, IPin dcPin, IPin resetPin, IPin busyPin) :
            base(device, spiBus, chipSelectPin, dcPin, resetPin, busyPin)
        { }

        public override uint Width => 128;
        public override uint Height => 296;
    }
}