using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Motion;
using Meadow.Units;
using System;
using System.Threading.Tasks;
using MU = Meadow.Units.MagneticField.UnitType;

namespace Sensors.Motion.Mmc5603_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        private Mmc5603 sensor;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            sensor = new Mmc5603(Device.CreateI2cBus());

            // classical .NET events can  be used
            sensor.Updated += (sender, result) =>
            {
                Resolver.Log.Info($"Magnetic Field: [X:{result.New.X.MicroTesla:N2}," +
                    $"Y:{result.New.Y.MicroTesla:N2}," +
                    $"Z:{result.New.Z.MicroTesla:N2} (MicroTeslas)]");
            };

            // Example that uses an IObservable subscription to only be notified when the filter is satisfied
            var consumer = Mmc5603.CreateObserver(
                handler: result => Resolver.Log.Info($"Observer: [x] changed by threshold; new [x]: X:{result.New.X.MicroTesla:N2}, old: X:{result.Old?.X.MicroTesla:N2}"),
                // only notify if there's a greater than 1 micro tesla on the Y axis
                filter: result =>
                {
                    if (result.Old is { } old)
                    {
                        return (result.New - old).Y > new MagneticField(1, MU.MicroTesla);
                    }
                    return false;
                });

            sensor.Subscribe(consumer);

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            //Read from sensor
            var result = await sensor.Read();

            //output initial readings text to console
            Resolver.Log.Info("Initial Readings:");
            Resolver.Log.Info(
                $"Magnetic field: [X:{result.X.MicroTesla:N2}," +
                $"Y:{result.Y.MicroTesla:N2}," +
                $"Z:{result.Z.MicroTesla:N2} (MicroTeslas)]");

            sensor.StartUpdating(TimeSpan.FromMilliseconds(1500));
        }

        //<!=SNOP=>
    }
}
