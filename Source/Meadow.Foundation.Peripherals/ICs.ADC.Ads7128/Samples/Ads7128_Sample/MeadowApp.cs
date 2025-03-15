using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.ADC;
using Meadow.Foundation.ICs.IOExpanders;
using System.Threading.Tasks;

namespace ADC.Ads7128_Sample
{
    public class MeadowApp : App<F7CoreComputeV2>
    {
        //<!=SNIP=>

        private Ads7128 adc;
        private Pcf8575 pcf1;
        private Pcf8575 pcf2;

        public override async Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            var bus = Device.CreateI2cBus();

            var address1 = Pcf8575.GetAddressForPins(false, false, false);
            var address2 = Pcf8575.GetAddressForPins(true, false, false);

            Resolver.Log.Info($"Address1: 0x{address1:X2}");
            Resolver.Log.Info($"Address2: 0x{address2:X2}");

            pcf1 = new Pcf8575(bus, address1);
            pcf2 = new Pcf8575(bus, address2);

            pcf1.AllOff();
            pcf2.AllOff();

            adc = new Ads7128(
                bus,
                Ads7128.Addresses.Default);

        }

        public override Task Run()
        {
            return base.Run();
        }

        //<!=SNOP=>
    }
}