using System;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Atmospheric;

namespace Sensors.AirQuality.Ccs811_Sample
{
    public class MeadowApp
        : App<F7Micro, MeadowApp>
    {
        Ccs811 sensor;

        public MeadowApp()
        {
            var bus = Device.CreateI2cBus(Meadow.Hardware.I2cBusSpeed.Fast);

            sensor = new Ccs811(bus);
            sensor.Updated += Sensor_Updated;
            sensor.StartUpdating();
        }

        private void Sensor_Updated(object sender, IChangeResult<(Meadow.Units.Concentration? CO2, Meadow.Units.Concentration? VOC)> e)
        {
            Console.WriteLine($"CO2: {e.New.CO2.Value.PartsPerMillion:n1}ppm, VOC: {e.New.VOC.Value.PartsPerBillion:n1}ppb");
        }

    }
}