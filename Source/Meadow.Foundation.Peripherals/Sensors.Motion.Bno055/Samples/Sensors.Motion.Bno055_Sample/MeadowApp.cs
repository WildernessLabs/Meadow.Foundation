using System;
using System.Threading.Tasks;
using Meadow;
using Meadow.Units;
using AU = Meadow.Units.Acceleration.UnitType;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Motion;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Bno055 sensor;

        public MeadowApp()
        {
            Console.WriteLine("Initializing");

            // create the sensor driver
            sensor = new Bno055(
                Device.CreateI2cBus(),
                Bno055.Addresses.LOW // use this if the address pin is pulled low
                //Bno055.Addresses.HIGH // use this if the address pin is pulled high
                );

            //==== Events
            // classical .NET events can also be used:
            sensor.Updated += (sender, result) => {
                Console.WriteLine($"Accel: [X:{result.New.Acceleration3D?.X.MetersPerSecondSquared:N2}," +
                    $"Y:{result.New.Acceleration3D?.Y.MetersPerSecondSquared:N2}," +
                    $"Z:{result.New.Acceleration3D?.Z.MetersPerSecondSquared:N2} (mps^2)]");

                Console.WriteLine($"Temp: {result.New.Temperature?.Celsius:N2}C");
            };

            //==== IObservable 
            // Example that uses an IObersvable subscription to only be notified
            // when the filter is satisfied
            var consumer = Bno055.CreateObserver(
                handler: result => {
                    Console.WriteLine($"Observer: [x] changed by threshold; new [x]: X:{result.New.Acceleration3D?.X.MetersPerSecondSquared:N2}, old: X:{result.Old?.Acceleration3D?.X.MetersPerSecondSquared:N2}");
                },
                // only notify if there's a greater than 1 micro tesla on the Y axis
                filter: result => {
                    if (result.Old is { } old) { //c# 8 pattern match syntax. checks for !null and assigns var.
                        return ((result.New.Acceleration3D - old.Acceleration3D)?.Y > new Acceleration(1, AU.MetersPerSecondSquared));
                    }
                    return false;
                }
                // if you want to always get notified, pass null for the filter:
                //filter: null
                );
            sensor.Subscribe(consumer);

            // debug
            sensor.DisplayRegisters();

            sensor.PowerMode = Bno055.PowerModes.NORMAL;
            //sensor.OperatingMode = Bno055.OperatingModes.CONFIGURATION_MODE;
            sensor.OperatingMode = Bno055.OperatingModes.NINE_DEGREES_OF_FREEDOM;

            Console.WriteLine($"IsFullyCalibrated: {sensor.IsFullyCalibrated}");

            // debug
            sensor.DisplayRegisters();

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

            Console.WriteLine($"Angular Accel: [X:{result.Acceleration3D?.X.MetersPerSecondSquared:N2}," +
                $"Y:{result.Acceleration3D?.Y.MetersPerSecondSquared:N2}," +
                $"Z:{result.Acceleration3D?.Z.MetersPerSecondSquared:N2} (dps^2)]");

            Console.WriteLine($"Temp: {result.Temperature?.Celsius:N2}C");
        }

    }
}