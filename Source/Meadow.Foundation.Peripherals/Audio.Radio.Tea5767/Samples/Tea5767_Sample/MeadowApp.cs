using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Audio.Radio;
using Meadow.Units;
using System.Threading.Tasks;

namespace Audio.Radio.Tea5767_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Tea5767 radio;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initializing...");

            radio = new Tea5767(Device.CreateI2cBus());

            return Task.CompletedTask;
        }

        public async override Task Run()
        {
            //scan through avaliable stations
            for (int i = 0; i < 8; i++)
            {
                await Task.Delay(1000);

                radio.SearchNextSilent();

                Resolver.Log.Info($"Current frequency: {radio.GetFrequency()}");
            }

            //set a known station
            radio.SelectFrequency(new Frequency(94.5, Frequency.UnitType.Megahertz));
        }

        //<!=SNOP=>
    }
}