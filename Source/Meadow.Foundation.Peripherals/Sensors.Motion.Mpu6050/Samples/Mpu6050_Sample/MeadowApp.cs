using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Motion;
using Meadow.Units;
using System;
using System.Threading.Tasks;
using AU = Meadow.Units.Acceleration.UnitType;

namespace Sensors.Motion.mpu5060_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Mpu6050 sensor;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            sensor = new Mpu6050(Device.CreateI2cBus());

            // classical .NET events can also be used:
            sensor.Updated += (sender, result) =>
            {
                Resolver.Log.Info($"Accel: [X:{result.New.Acceleration3D?.X.MetersPerSecondSquared:N2}," +
                    $"Y:{result.New.Acceleration3D?.Y.MetersPerSecondSquared:N2}," +
                    $"Z:{result.New.Acceleration3D?.Z.MetersPerSecondSquared:N2} (m/s^2)]");

                Resolver.Log.Info($"Angular Velocity: [X:{result.New.AngularVelocity3D?.X.DegreesPerSecond:N2}," +
                    $"Y:{result.New.AngularVelocity3D?.Y.DegreesPerSecond:N2}," +
                    $"Z:{result.New.AngularVelocity3D?.Z.DegreesPerSecond:N2} (dps)]");

                Resolver.Log.Info($"Temp: {result.New.Temperature?.Celsius:N2}C");
            };

            // Example that uses an IObservable subscription to only be notified when the filter is satisfied
            var consumer = Mpu6050.CreateObserver(
                handler: result => Resolver.Log.Info($"Observer: [x] changed by threshold; new [x]: X:{result.New.Acceleration3D?.X:N2}, old: X:{result.Old?.Acceleration3D?.X:N2}"),
                // only notify if there's a greater than 1G change in the Z direction
                filter: result =>
                {
                    if (result.Old is { } old)
                    {
                        return ((result.New.Acceleration3D.Value - old.Acceleration3D.Value).Z > new Acceleration(1, AU.Gravity));
                    }
                    return false;
                });
            sensor.Subscribe(consumer);

            return Task.CompletedTask;
        }

        public async override Task Run()
        {
            //==== one-off read
            var result = await sensor.Read();

            Resolver.Log.Info($"Accel: [X:{result.Acceleration3D?.X.MetersPerSecondSquared:N2}," +
                $"Y:{result.Acceleration3D?.Y.MetersPerSecondSquared:N2}," +
                $"Z:{result.Acceleration3D?.Z.MetersPerSecondSquared:N2} (m/s^2)]");

            Resolver.Log.Info($"Angular Accel: [X:{result.AngularVelocity3D?.X.DegreesPerSecond:N2}," +
                $"Y:{result.AngularVelocity3D?.Y.DegreesPerSecond:N2}," +
                $"Z:{result.AngularVelocity3D?.Z.DegreesPerSecond:N2} (dps)]");

            Resolver.Log.Info($"Temp: {result.Temperature?.Celsius:N2}C");

            sensor.StartUpdating(TimeSpan.FromMilliseconds(500));
        }

        //<!=SNOP=>
    }
}