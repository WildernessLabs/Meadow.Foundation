using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Motion;
using Meadow.Units;

namespace Sensors.Motion.mpu5060_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Mpu6050 sensor;

        public MeadowApp()
        {
            Console.WriteLine("Initializing");

            // Mpu5060 I2C address could be 0x68 or 0x69
            sensor = new Mpu6050(Device.CreateI2cBus(), 0x69);

            //==== Events
            // classical .NET events can also be used:
            sensor.Updated += (object sender, IChangeResult<(Acceleration3D? Acceleration, AngularAcceleration3D? AngularAcceleration)> e) => {
                Console.WriteLine($"Accel: [X:{e.New.Acceleration?.X.MetersPerSecondSquared:N2}," +
                    $"Y:{e.New.Acceleration?.Y.MetersPerSecondSquared:N2}," +
                    $"Z:{e.New.Acceleration?.Z.MetersPerSecondSquared:N2} (mps^2)]");

                Console.WriteLine($"Angular Accel: [X:{e.New.AngularAcceleration?.X.DegreesPerSecondSquared:N2}," +
                    $"Y:{e.New.AngularAcceleration?.Y.DegreesPerSecondSquared:N2}," +
                    $"Z:{e.New.AngularAcceleration?.Z.DegreesPerSecondSquared:N2} (dps^2)]");
            };

            //==== IObservable 
            // Example that uses an IObersvable subscription to only be notified
            // when the temperature changes by at least a degree, and humidty by 5%.
            // (blowing hot breath on the sensor should trigger)
            // TODO/BUG: uncommenting this means no events are raised.
            //var consumer = Mpu6050.CreateObserver(
            //    handler: result => {
            //        Console.WriteLine($"Observer: [x] changed by threshold; new [x]: X:{result.New.Acceleration3D?.X:N2}, old: X:{result.Old?.Acceleration3D?.X:N2}");
            //    },
            //    // only notify if the change is greater than 0.5°C
            //    filter: result => {
            //        if (result.Old is { } old) { //c# 8 pattern match syntax. checks for !null and assigns var.
            //            return (
            //            (result.New.Acceleration3D.Value - old.Acceleration3D.Value).X > 0.1 // returns true if > 0.1 X change.
            //            // can add addtional constraints, too:
            //            //&&
            //            //(result.New.AngularAcceleration3D.Value - old.AngularAcceleration3D.Value).X > 0.05 // 
            //            );
            //        }
            //        return false;
            //    }
            //    // if you want to always get notified, pass null for the filter:
            //    //filter: null
            //    );
            //sensor.Subscribe(consumer);

            //==== one-off read
            //ReadConditions().Wait();

            // start updating
            sensor.StartUpdating(500);


            while (true)
            {
                Console.WriteLine($"{sensor.Temperature.Celsius:n2}C");
                Thread.Sleep(5000);
            }
        }

        // TODO: uncomment when there is a `Read()` method
        //protected async Task ReadConditions()
        //{
        //    var conditions = await sensor.Read();
        //    Console.WriteLine("Initial Readings:");
        //    Console.WriteLine($"Accel: [X:{conditions.New.Acceleration?.X.MetersPerSecondSquared:N2}," +
        //        $"Y:{conditions.New.Acceleration?.Y.MetersPerSecondSquared:N2}," +
        //        $"Z:{conditions.New.Acceleration?.Z.MetersPerSecondSquared:N2} (mps^2)]");

        //    Console.WriteLine($"Angular Accel: [X:{conditions.New.AngularAcceleration?.X.DegreesPerSecondSquared:N2}," +
        //        $"Y:{conditions.New.AngularAcceleration?.Y.DegreesPerSecondSquared:N2}," +
        //        $"Z:{conditions.New.AngularAcceleration?.Z.DegreesPerSecondSquared:N2} (dps^2)]");
        //}

    }
}