using System;
using System.Threading.Tasks;
using Meadow;
using Meadow.Units;
using AU = Meadow.Units.Acceleration.UnitType;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Motion;

namespace Sensors.Motion.Adxl362_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Adxl362 sensor;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            // create the sensor driver
            sensor = new Adxl362(Device, Device.CreateSpiBus(), Device.Pins.D00);

            // classical .NET events can also be used:
            sensor.Updated += (sender, result) => {
                Resolver.Log.Info($"Accel: [X:{result.New.Acceleration3D?.X.MetersPerSecondSquared:N2}," +
                    $"Y:{result.New.Acceleration3D?.Y.MetersPerSecondSquared:N2}," +
                    $"Z:{result.New.Acceleration3D?.Z.MetersPerSecondSquared:N2} (m/s^2)]");

                Resolver.Log.Info($"Temp: {result.New.Temperature?.Celsius:N2}C");
            };

            // Example that uses an IObservable subscription to only be notified when the filter is satisfied
            var consumer = Adxl362.CreateObserver(
                handler: result => Resolver.Log.Info($"Observer: [x] changed by threshold; new [x]: X:{result.New.Acceleration3D?.X.MetersPerSecondSquared:N2}, old: X:{result.Old?.Acceleration3D?.X.MetersPerSecondSquared:N2}"),
                filter: result => {
                    if (result.Old is { } old) { //c# 8 pattern match syntax. checks for !null and assigns var.
                        return ((result.New.Acceleration3D - old.Acceleration3D)?.Y > new Acceleration(1, AU.MetersPerSecondSquared));
                    }
                    return false;
                });
            sensor.Subscribe(consumer);

            return Task.CompletedTask;
        }

        public async override Task Run()
        {
            Resolver.Log.Info($"Device ID: {sensor.DeviceID}");

            var result = await sensor.Read();
            Resolver.Log.Info("Initial Readings:");
            Resolver.Log.Info($"Accel: [X:{result.Acceleration3D?.X.MetersPerSecondSquared:N2}," +
                $"Y:{result.Acceleration3D?.Y.MetersPerSecondSquared:N2}," +
                $"Z:{result.Acceleration3D?.Z.MetersPerSecondSquared:N2} (m/s^2)]");

            Resolver.Log.Info($"Temp: {result.Temperature?.Celsius:N2}C");

            sensor.StartUpdating(TimeSpan.FromMilliseconds(1000));
        }

        //<!=SNOP=>
    }
}