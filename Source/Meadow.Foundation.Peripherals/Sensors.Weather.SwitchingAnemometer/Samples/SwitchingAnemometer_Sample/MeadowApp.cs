using System.IO;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Weather;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Units;

namespace MeadowApp
{
    public class MeadowApp : App<F7CoreComputeV2>
    {
        //<!=SNIP=>

        private SwitchingAnemometer anemometer;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            anemometer = new SwitchingAnemometer(Device.Pins.A01);

            // Uncomment to test SwitchingAnemometer implementation.
            // Assumes external wire connected between a PWM capable output and the above configured 
            // SwitchingAnemometer input. 
            //double speed = 24; // km/hr
            //double frequency = speed / (anemometer.KmhPerSwitchPerSecond);
            //IPwmPort pwm = Device.CreatePwmPort(Device.Pins.D20, new Frequency(frequency, Frequency.UnitType.Hertz));
            //pwm.Start();

            //==== classic events example
            anemometer.Updated += (sender, result) =>
            {
                Resolver.Log.Info($"new speed: {result.New.KilometersPerHour:n1}kmh, old: {result.Old?.KilometersPerHour:n1}kmh");
            };

            //==== IObservable example
            var observer = SwitchingAnemometer.CreateObserver(
                handler: result =>
                {
                    Resolver.Log.Info($"new speed (from observer): {result.New.KilometersPerHour:n1}kmh, old: {result.Old?.KilometersPerHour:n1}kmh");
                },
                null
                );
            anemometer.Subscribe(observer);

            return Task.CompletedTask;
        }

        public override Task Run()
        {
            // start raising updates
            anemometer.StartUpdating();
            Resolver.Log.Info("Hardware initialized.");

            return Task.CompletedTask;
        }

        //<!=SNOP=>
    }
}