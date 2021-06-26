using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Motion;
using Meadow.Units;
using AU = Meadow.Units.Acceleration.UnitType;

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
                    $"Z:{result.New.Acceleration3D?.Z.MetersPerSecondSquared:N2} (m/s^2)]");

                Console.WriteLine($"Angular Velocity: [X:{result.New.AngularVelocity3D?.X.DegreesPerSecond:N2}," +
                    $"Y:{result.New.AngularVelocity3D?.Y.DegreesPerSecond:N2}," +
                    $"Z:{result.New.AngularVelocity3D?.Z.DegreesPerSecond:N2} (dps)]");

                Console.WriteLine($"Temp: {result.New.Temperature?.Celsius:N2}C");
            };

            //==== IObservable 
            // Example that uses an IObersvable subscription to only be notified
            // when the filter is satisfied
            var consumer = Mpu6050.CreateObserver(
                handler: result => {
                    Console.WriteLine($"Observer: [x] changed by threshold; new [x]: X:{result.New.Acceleration3D?.X:N2}, old: X:{result.Old?.Acceleration3D?.X:N2}");
                },
                // only notify if there's a greater than 1G change in the Z direction
                filter: result => {
                    if (result.Old is { } old) { //c# 8 pattern match syntax. checks for !null and assigns var.
                        return ((result.New.Acceleration3D.Value - old.Acceleration3D.Value).Z > new Acceleration(1, AU.Gravity));
                    }
                    return false;
                }
                // if you want to always get notified, pass null for the filter:
                //filter: null
                );
            sensor.Subscribe(consumer);

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
                $"Z:{result.Acceleration3D?.Z.MetersPerSecondSquared:N2} (m/s^2)]");

            Console.WriteLine($"Angular Accel: [X:{result.AngularVelocity3D?.X.DegreesPerSecond:N2}," +
                $"Y:{result.AngularVelocity3D?.Y.DegreesPerSecond:N2}," +
                $"Z:{result.AngularVelocity3D?.Z.DegreesPerSecond:N2} (dps)]");

            Console.WriteLine($"Temp: {result.Temperature?.Celsius:N2}C");
        }

    }
}