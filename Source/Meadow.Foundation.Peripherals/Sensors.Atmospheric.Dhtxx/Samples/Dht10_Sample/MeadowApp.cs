using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Atmospheric.Dhtxx;

namespace Dht10_Sample
{
    public class MeadowApp : App<F7MicroV2, MeadowApp>
    {
        Dht10 dht10;

        public MeadowApp()
        {
            dht10 = new Dht10(Device.CreateI2cBus());
        }
    }
}