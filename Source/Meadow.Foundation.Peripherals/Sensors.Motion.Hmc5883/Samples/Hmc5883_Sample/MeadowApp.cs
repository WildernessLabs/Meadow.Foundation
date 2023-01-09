using System;
using System.Threading.Tasks;
using Meadow;
using Meadow.Units;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Motion;

namespace MeadowApp
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Hmc5883 sensor;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            sensor = new Hmc5883(Device.CreateI2cBus());

            // classical .NET events can also be used:
            sensor.Updated += (sender, result) => {
                Resolver.Log.Info($"Direction: [X:{result.New.X:N2}," +
                    $"Y:{result.New.Y:N2}," +
                    $"Z:{result.New.Z:N2}]");

                Resolver.Log.Info($"Heading: [{Hmc5883.DirectionToHeading(result.New).DecimalDegrees:N2}] degrees");
            };

            // Example that uses an IObservable subscription to only be notified when the filter is satisfied
            var consumer = Hmc5883.CreateObserver(
                handler: result => Resolver.Log.Info($"Observer: [x] changed by threshold; new [x]: X:{Hmc5883.DirectionToHeading(result.New):N2}," +
                        $" old: X:{((result.Old != null) ? Hmc5883.DirectionToHeading(result.Old.Value) : "n/a"):N2} degrees"),
                // only notify if there's a greater than 5° of heading change
                filter: result => {
                    if (result.Old is { } old) { //c# 8 pattern match syntax. checks for !null and assigns var.
                        return (Hmc5883.DirectionToHeading(result.New - old) > new Azimuth(5));
                    }
                    return false;
                });

            sensor.Subscribe(consumer);

            return Task.CompletedTask;
        }

        public async override Task Run()
        {
            var result = await sensor.Read();
            Resolver.Log.Info("Initial Readings:");
            Resolver.Log.Info($"Direction: [X:{result.X:N2}," +
                $"Y:{result.Y:N2}," +
                $"Z:{result.Z:N2}]");

            Resolver.Log.Info($"Heading: [{Hmc5883.DirectionToHeading(result).DecimalDegrees:N2}] degrees");

            sensor.StartUpdating(TimeSpan.FromMilliseconds(1000));
        }

        //<!=SNOP=>
    }
}