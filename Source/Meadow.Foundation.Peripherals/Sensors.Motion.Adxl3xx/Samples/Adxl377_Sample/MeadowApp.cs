using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Motion;
using Meadow.Units;
using System;
using System.Threading.Tasks;
using AU = Meadow.Units.Acceleration.UnitType;

namespace Sensors.Motion.Adxl377_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Adxl377 sensor;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            // create the sensor driver
            sensor = new Adxl377(Device.Pins.A00, Device.Pins.A01, Device.Pins.A02, null);

            // classical .NET events can also be used:
            sensor.Updated += (sender, result) =>
            {
                Resolver.Log.Info($"Accel: [X:{result.New.X.MetersPerSecondSquared:N2}," +
                    $"Y:{result.New.Y.MetersPerSecondSquared:N2}," +
                    $"Z:{result.New.Z.MetersPerSecondSquared:N2} (m/s^2)]");
            };

            // Example that uses an IObservable subscription to only be notified when the filter is satisfied
            var consumer = Adxl377.CreateObserver(
                handler: result => Resolver.Log.Info($"Observer: [x] changed by threshold; new [x]: X:{result.New.X:N2}, old: X:{result.Old?.X:N2}"),
                // only notify if there's a greater than 1G change in the Z direction
                filter: result =>
                {
                    if (result.Old is { } old)
                    { //c# 8 pattern match syntax. checks for !null and assigns var.
                        return ((result.New - old).Z > new Acceleration(1, AU.Gravity));
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
            Resolver.Log.Info($"Accel: [X:{result.X.MetersPerSecondSquared:N2}," +
                $"Y:{result.Y.MetersPerSecondSquared:N2}," +
                $"Z:{result.Z.MetersPerSecondSquared:N2} (m/s^2)]");

            sensor.StartUpdating(TimeSpan.FromMilliseconds(500));
        }

        //<!=SNOP=>
    }
}