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
            sensor = new Mpu6050(
                Device.CreateI2cBus(),
                Mpu6050.Addresses.High // Address pin pulled high
                //Mpu6050.Addresses.Low // Address pin pulled low.
                );

            //==== Events
            // classical .NET events can also be used:
            sensor.Updated += (sender, result) => {
                Console.WriteLine($"Accel: [X:{result.New.Acceleration3D?.X.MetersPerSecondSquared:N2}," +
                    $"Y:{result.New.Acceleration3D?.Y.MetersPerSecondSquared:N2}," +
                    $"Z:{result.New.Acceleration3D?.Z.MetersPerSecondSquared:N2} (mps^2)]");

                Console.WriteLine($"Angular Accel: [X:{result.New.AngularAcceleration3D?.X.DegreesPerSecondSquared:N2}," +
                    $"Y:{result.New.AngularAcceleration3D?.Y.DegreesPerSecondSquared:N2}," +
                    $"Z:{result.New.AngularAcceleration3D?.Z.DegreesPerSecondSquared:N2} (dps^2)]");

                Console.WriteLine($"Temp: {result.New.Temperature?.Celsius:N2}C");
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
            ReadConditions().Wait();

            // start updating
            sensor.StartUpdating(TimeSpan.FromMilliseconds(500));
        }

        protected async Task ReadConditions()
        {
            var result = await sensor.Read();
            Console.WriteLine("Initial Readings:");
            Console.WriteLine($"Accel: [X:{result.Acceleration3D?.X.MetersPerSecondSquared:N2}," +
                $"Y:{result.Acceleration3D?.Y.MetersPerSecondSquared:N2}," +
                $"Z:{result.Acceleration3D?.Z.MetersPerSecondSquared:N2} (mps^2)]");

            Console.WriteLine($"Angular Accel: [X:{result.AngularAcceleration3D?.X.DegreesPerSecondSquared:N2}," +
                $"Y:{result.AngularAcceleration3D?.Y.DegreesPerSecondSquared:N2}," +
                $"Z:{result.AngularAcceleration3D?.Z.DegreesPerSecondSquared:N2} (dps^2)]");

            Console.WriteLine($"Temp: {result.Temperature?.Celsius:N2}C");
        }

    }
}