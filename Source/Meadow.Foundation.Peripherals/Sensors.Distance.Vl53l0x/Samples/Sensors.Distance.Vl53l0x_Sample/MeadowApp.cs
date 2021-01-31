using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Distance;
using Meadow.Hardware;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Sensors.Distance.Vl53l0x_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        Vl53l0x sensor;

        public MeadowApp()
        {
            Initialize();

            sensor.Updated += Sensor_Updated;
            sensor.StartUpdating();
        }

        private void Sensor_Updated(object sender, Meadow.Peripherals.Sensors.Distance.DistanceConditionChangeResult e)
        {
            if (e.New == null || e.New.Distance == null)
            {
                return;
            }

            Console.WriteLine($"{e.New.Distance.Value}mm");
        }

        void Initialize()
        {
            Console.WriteLine("Initialize hardware...");
            var i2cBus = Device.CreateI2cBus(I2cBusSpeed.FastPlus);
            sensor = new Vl53l0x(Device, i2cBus);
        }

        void InitializeWithShutdownPin()
        {
            Console.WriteLine("Initialize hardware...");
            var i2cBus = Device.CreateI2cBus(I2cBusSpeed.FastPlus);
            sensor = new Vl53l0x(Device, i2cBus, Device.Pins.D05);
        }
    }
}