using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.ADC;
using System.Threading.Tasks;

namespace ADC.Ad7768_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        private Ad7768 adc;

        public override async Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            adc = new Ad7768(
                Device.CreateSpiBus(),
                Device.Pins.D05,
                null, null,
                Device.Pins.D06);

            Resolver.Log.Info("Reading...");

            adc.Initialize();

            while (true)
            {
                adc.Test();
                await Task.Delay(1000);
            }
        }

        //<!=SNOP=>
    }
}