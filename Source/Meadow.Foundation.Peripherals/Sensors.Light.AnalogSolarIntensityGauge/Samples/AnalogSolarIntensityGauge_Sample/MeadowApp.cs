using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Hardware;
using Meadow.Foundation.Sensors.Light;
using System.Threading.Tasks;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        IAnalogInputPort analogIn;
        AnalogSolarIntensityGauge solarGauge;

        public MeadowApp()
        {
            Initialize();

            // do a one-off read
            ReadSolarIntensityGauge().Wait();

            // start updating
            Start();
       }

        void Initialize()
        {
            Console.WriteLine("Initialize hardware...");

            Console.WriteLine("Creating analog input port.");
            analogIn = Device.CreateAnalogInputPort(Device.Pins.A02);

            solarGauge = new AnalogSolarIntensityGauge(analogIn);

            //==== classic .NET Event
            solarGauge.SolarIntensityUpdated += (s, result) => {
                Console.WriteLine($"SolarIntensityUpdated: {result.New * 100:n2}%");
            };

            //==== Filterable observer
            var observer = AnalogSolarIntensityGauge.CreateObserver(
                handler: result => Console.WriteLine($"Observer filter satisifed, new intensity: {result.New * 100:n2}%"),
                filter: result => {
                    if (result.Old is { } old) {
                        return (Math.Abs(result.New - old) > 0.05); // only notify if change is > 5%
                    } return false; }
                );
            solarGauge.Subscribe(observer);

            Console.WriteLine("Hardware initialized.");
        }

        void Start()
        {
            solarGauge.StartUpdating(standbyDuration: 500);
        }

        async Task ReadSolarIntensityGauge()
        {
            var result = await solarGauge.Read();
            Console.WriteLine($"Solar Intensity: {result.New * 100:n2}%");
        }
    }
}