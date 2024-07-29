using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Temperature;
using System;
using System.Threading.Tasks;

namespace Sensors.Temperature.Adt7410_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Adt7410 adt7410;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            adt7410 = new Adt7410(Device.CreateI2cBus());
            adt7410.SensorResolution = Adt7410.Resolution.Resolution13Bits;

            var consumer = Adt7410.CreateObserver(
                handler: result =>
                {
                    Resolver.Log.Info($"Temperature New {result.New.Celsius:N2}C, Old {result.Old?.Celsius:N2}C");
                },
                filter: null
            );
            adt7410.Subscribe(consumer);

            adt7410.Updated += (object sender, IChangeResult<Meadow.Units.Temperature> e) =>
            {
                Resolver.Log.Info($"Temperature Updated: {e.New.Celsius:N2}C");
            };

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            var temp = await adt7410.Read();
            Resolver.Log.Info($"Current temperature: {temp.Celsius}C");

            adt7410.StartUpdating(TimeSpan.FromSeconds(1));
        }

        //<!=SNOP=>
    }
}