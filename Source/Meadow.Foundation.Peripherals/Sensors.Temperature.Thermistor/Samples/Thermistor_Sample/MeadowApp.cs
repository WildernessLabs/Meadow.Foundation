using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Temperature;
using Meadow.Gateways.Bluetooth;
using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Sensors.Temperature.Thermistor_Sample
{
    public class MeadowApp : App<F7CoreComputeV2>
    {
        //<!=SNIP=>

        private SteinhartHartCalculatedThermistor thermistor;
        private IProjectLabHardware projectLab;
        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");
            projectLab = ProjectLab.Create();
            thermistor = new SteinhartHartCalculatedThermistor(projectLab.GroveAnalog.Pins.D0.CreateAnalogInputPort(10), new Resistance(10, Meadow.Units.Resistance.UnitType.Kiloohms));

            var consumer = SteinhartHartCalculatedThermistor.CreateObserver(
            handler: result =>
            {
                Resolver.Log.Info($"Handler Temperature New Value: {result.New.Celsius:N1} °C");
                Resolver.Log.Info($"Handler From Thermistor {thermistor.Conditions.Celsius:N1} °C");
                },
                filter: null
            );
            thermistor.Subscribe(consumer);

            thermistor.Updated += Thermistor_TempreatureUpdated;
            /*
            (object sender, IChangeResult<Meadow.Units.Temperature> e) =>
            {
                //Resolver.Log.Info($"Updated Temperature: {e.New.Celsius:N1} °C");

            };
            */
            return base.Initialize();
        }

        
        public override async Task Run()
        {
            var temp = await thermistor.Read();
            Resolver.Log.Info($"Initial temperature: {temp.Fahrenheit:N1}F/{temp.Celsius:N1}C");
            Resolver.Log.Info($"Inital From therm: {thermistor.Conditions.Fahrenheit:N1}F/{thermistor.Conditions.Celsius:N1}C");
            thermistor.StartUpdating(TimeSpan.FromSeconds(1d));
        }


        // function called when using the IChangeResult interface when temp measure is updated
        private void Thermistor_TempreatureUpdated(object sender, IChangeResult<Meadow.Units.Temperature> e)
        {
            Resolver.Log.Info($"IChangeResultTemperature: {e.New.Celsius:N1}°C");
            Resolver.Log.Info($"IChangeResult from Thermistor: {thermistor.Conditions.Celsius:N1}°C");
        }

        //<!=SNOP=>
    }
   
}