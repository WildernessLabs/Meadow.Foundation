//#define DEVICE_FEATHER
#define DEVICE_PROJECT_LAB
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Distance;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace A02yyuw_Sample
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
#if DEVICE_FEATHER          // Feather: Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7FeatherV2>
#endif
#if DEVICE_PROJECT_LAB      //ProjectLab: uses an F7CoreComputeV2 board
    public class MeadowApp : App<F7CoreComputeV2>
#endif
    {
        //<!=SNIP=>

        A02yyuw a02yyuw;
        protected TimeSpan updateInterval = TimeSpan.FromSeconds(10);
        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            a02yyuw = new A02yyuw(Device, Device.PlatformOS.GetSerialPortName("COM4"), A02yyuw.MODE_UART_CONTROL);

            /******* Using the Observer/consumer approach, with optional filter************
            var consumer = A02yyuw.CreateObserver(
                handler: result =>
                {
                    Resolver.Log.Info($"Observer: Distance changed by threshold; new distance: {result.New.Centimeters:N1}cm, old: {result.Old?.Centimeters:N1}cm");
                },
                filter: result =>
                {
                    if (result.Old is { } old)
                    {
                        return Math.Abs((result.New - old).Centimeters) > 5.0;
                    }
                    return false;
                }
            );
            a02yyuw.Subscribe(consumer);

            /********************** /Using the IChangeResult interface *******************
            a02yyuw.Updated += A02yyuw_DistanceUpdated;
            ********************** IChangeResult interface *******************/
            return Task.CompletedTask;
        }


        public override async Task Run()
        {
            Resolver.Log.Info("Run...");
            var distance = await a02yyuw.Read(); // read once and log
            Resolver.Log.Info($"Initial distance is: {distance.Centimeters:N1}cm");

            a02yyuw.StartUpdating(TimeSpan.FromSeconds(2));
            while (true)
            {
                Resolver.Log.Info($"Last Distance: {a02yyuw.Conditions.Centimeters:N1} cm");
                await Task.Delay(updateInterval);
            }
        }

        // function called when using the IChangeResult interface when distance measure is updated
        private void A02yyuw_DistanceUpdated(object sender, IChangeResult<Length> e)
        {
            Resolver.Log.Info($"Distance: {e.New.Centimeters:N1}cm");
        }
        //<!=SNOP=>
    }
}