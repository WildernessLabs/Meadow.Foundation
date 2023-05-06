using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Distance;
using Meadow.Hardware;
using Meadow.Units;
using System;
using System.Threading.Tasks;
using LU = Meadow.Units.Length.UnitType;

namespace Sensors.Distance.Vl53l0x_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Vl53l0x sensor;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initializing hardware...");

            var i2cBus = Device.CreateI2cBus(I2cBusSpeed.FastPlus);
            sensor = new Vl53l0x(i2cBus);

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
                Resolver.Log.Info("out of range.");
            }
            else
            {
                Resolver.Log.Info($"{result.New.Millimeters}mm / {result.New.Inches:n3}\"");
            }
        }

        //<!=SNOP=>

        void InitializeWithShutdownPin()
        {
            Resolver.Log.Info("Initialize...");
            var i2cBus = Device.CreateI2cBus(I2cBusSpeed.FastPlus);
            sensor = new Vl53l0x(i2cBus, Device.Pins.D05, 250);
        }
    }
}