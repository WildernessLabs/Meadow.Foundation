using System;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Distance;
using Meadow.Hardware;
using Meadow.Units;
using LU = Meadow.Units.Length.UnitType;

namespace Sensors.Distance.Vl53l0x_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Vl53l0x sensor;

        public override Task Initialize()
        {
            Console.WriteLine("Initializing hardware...");

            var i2cBus = Device.CreateI2cBus(I2cBusSpeed.FastPlus);
            sensor = new Vl53l0x(Device, i2cBus, (byte)Vl53l0x.Addresses.Default);

            sensor.DistanceUpdated += Sensor_Updated;

            return Task.CompletedTask;
        }

        public override Task Run()
        {
            sensor.StartUpdating(TimeSpan.FromMilliseconds(250));

            return Task.CompletedTask;
        }

        private void Sensor_Updated(object sender, IChangeResult<Length> result)
        {
            if (result.New == null) { return; }

            if (result.New < new Length(0, LU.Millimeters))
            { 
                Console.WriteLine("out of range.");
            }
            else 
            {
                Console.WriteLine($"{result.New.Millimeters}mm / {result.New.Inches:n3}\"");
            }
        }

        //<!=SNOP=>

        void InitializeWithShutdownPin()
        {
            Console.WriteLine("Initialize...");
            var i2cBus = Device.CreateI2cBus(I2cBusSpeed.FastPlus);
            sensor = new Vl53l0x(Device, i2cBus, Device.Pins.D05, 250);
        }
    }
}