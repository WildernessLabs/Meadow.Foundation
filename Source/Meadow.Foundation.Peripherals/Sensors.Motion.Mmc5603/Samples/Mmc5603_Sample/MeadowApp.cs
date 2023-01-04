using System;
using System.Threading.Tasks;
using Meadow;
using Meadow.Units;
using MU = Meadow.Units.MagneticField.UnitType;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Motion;
using System.Threading;

namespace Sensors.Motion.Mmc5603_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Mmc5603 sensor;

        public override Task Initialize()
        {
            Console.WriteLine("Initialize...");

            sensor = new Mmc5603(Device.CreateI2cBus());

            Console.WriteLine("a...");

            // classical .NET events can  be used
            sensor.Updated += (sender, result) => 
            {
                Console.WriteLine($"Magnetic Field: [X:{result.New.MagneticField3D?.X.MicroTesla:N2}," +
                    $"Y:{result.New.MagneticField3D?.Y.MicroTesla:N2}," +
                    $"Z:{result.New.MagneticField3D?.Z.MicroTesla:N2} (MicroTeslas)]");

                Console.WriteLine($"Temp: {result.New.Temperature?.Celsius:N2}C");
            };

            Console.WriteLine("b...");

            // Example that uses an IObservable subscription to only be notified when the filter is satisfied
            var consumer = Mmc5603.CreateObserver(
                handler: result => Console.WriteLine($"Observer: [x] changed by threshold; new [x]: X:{result.New.MagneticField3D?.X.MicroTesla:N2}, old: X:{result.Old?.MagneticField3D?.X.MicroTesla:N2}"),
                // only notify if there's a greater than 1 micro tesla on the Y axis
                filter: result => 
                {
                    if (result.Old is { } old) 
                    { //c# 8 pattern match syntax. checks for !null and assigns var
                        return (result.New.MagneticField3D - old.MagneticField3D)?.Y > new MagneticField(1, MU.MicroTesla);
                    }
                    return false;
                });

            sensor.Subscribe(consumer);

            return Task.CompletedTask;
        }

        public async override Task Run()
        {
            Resolver.Log.Loglevel = Meadow.Logging.LogLevel.Trace;

            //Read from sensor
            var result = await sensor.Read();

            //output initial readings text to console
            Console.WriteLine("Initial Readings:");
            Console.WriteLine(
                $"Mangetic field: [X:{result.MagneticField3D?.X.MicroTesla:N2}," +
                $"Y:{result.MagneticField3D?.Y.MicroTesla:N2}," +
                $"Z:{result.MagneticField3D?.Z.MicroTesla:N2} (microteslas)]");

            Console.WriteLine($"Temp: {result.Temperature?.Celsius:N2}C");

            sensor.StartUpdating(TimeSpan.FromMilliseconds(1500));

            Thread.Sleep(15000);
        }

        //<!=SNOP=>
    }
}