using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.ADC;
using Meadow.Hardware;
using System.Threading.Tasks;

namespace ADC.Ads7128_Sample
{
    public class MeadowApp : App<F7CoreComputeV2>
    {
        //<!=SNIP=>

        private Ads7128 adc;
        private IAnalogInputPort ch0;

        public override async Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            var bus = Device.CreateI2cBus();

            adc = new Ads7128(
                bus,
                Ads7128.Addresses.Default);

            ch0 = adc.CreateAnalogInputPort(adc.Pins.AIN0);
        }

        public override async Task Run()
        {
            while (true)
            {
                var voltage = await ch0.Read();

                Resolver.Log.Info($"AIN0 voltage: {voltage.Volts:N2}");

                await Task.Delay(2000);
            }
        }

        //<!=SNOP=>
    }
}