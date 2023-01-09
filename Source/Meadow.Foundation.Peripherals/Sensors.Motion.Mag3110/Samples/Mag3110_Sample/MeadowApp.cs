using System;
using System.Threading.Tasks;
using Meadow;
using Meadow.Units;
using MU = Meadow.Units.MagneticField.UnitType;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Motion;

namespace MeadowApp
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Mag3110 sensor;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            sensor = new Mag3110(Device.CreateI2cBus());

            // classical .NET events can  be used
            sensor.Updated += (sender, result) => {
                Resolver.Log.Info($"Magnetic Field: [X:{result.New.MagneticField3D?.X.MicroTesla:N2}," +
                    $"Y:{result.New.MagneticField3D?.Y.MicroTesla:N2}," +
                    $"Z:{result.New.MagneticField3D?.Z.MicroTesla:N2} (MicroTeslas)]");

                Resolver.Log.Info($"Temp: {result.New.Temperature?.Celsius:N2}C");
            };

            // Example that uses an IObservable subscription to only be notified when the filter is satisfied
            var consumer = Mag3110.CreateObserver(
                handler: result => Resolver.Log.Info($"Observer: [x] changed by threshold; new [x]: X:{result.New.MagneticField3D?.X.MicroTesla:N2}, old: X:{result.Old?.MagneticField3D?.X.MicroTesla:N2}"),
                // only notify if there's a greater than 1 micro tesla on the Y axis
                filter: result => {
                    if (result.Old is { } old) { //c# 8 pattern match syntax. checks for !null and assigns var.
                        return ((result.New.MagneticField3D - old.MagneticField3D)?.Y > new MagneticField(1, MU.MicroTesla));
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
            Resolver.Log.Info($"Mangetic field: [X:{result.MagneticField3D?.X.MicroTesla:N2}," +
                $"Y:{result.MagneticField3D?.Y.MicroTesla:N2}," +
                $"Z:{result.MagneticField3D?.Z.MicroTesla:N2} (microteslas)]");

            Resolver.Log.Info($"Temp: {result.Temperature?.Celsius:N2}C");

            sensor.StartUpdating(TimeSpan.FromMilliseconds(500));
        }

        //<!=SNOP=>
    }
}