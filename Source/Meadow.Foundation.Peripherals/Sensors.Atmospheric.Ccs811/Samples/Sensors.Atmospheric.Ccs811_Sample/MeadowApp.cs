using System;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Atmospheric;

namespace Sensors.AirQuality.Ccs811_Sample
{
    public class MeadowApp
#if !JETSON
        : App<MeadowOnLinux<JetsonNanoPinout>, MeadowApp>
#else
        : App<F7Micro, MeadowApp>
#endif
    {
        Ccs811 sensor;

        public MeadowApp()
        {
#if !JETSON
            var bus = Device.CreateI2cBus(1);
#else
            var bus = Device.CreateI2cBus(Meadow.Hardware.I2cBusSpeed.Fast);
#endif            
            sensor = new Ccs811(bus);
            sensor.Updated += Sensor_Updated; ;
            sensor.StartUpdating();

            //var bl = sensor.GetBaseline();
            //Console.WriteLine($"Baseline A: 0x{bl:x4}");
            //sensor.SetBaseline(0x847b);
            //bl = sensor.GetBaseline();
            //Console.WriteLine($"Baseline B: 0x{bl:x4}");
            //sensor.SetMeasurementMode(Ccs811.MeasurementMode.ConstantPower250ms);
        }

        private void Sensor_Updated(object sender, IChangeResult<(Meadow.Units.Concentration? CO2, Meadow.Units.Concentration? VOC)> e)
        {
            Console.WriteLine($"CO2: {e.New.CO2.Value.PartsPerMillion:n1}ppm, VOC: {e.New.VOC.Value.PartsPerBillion:n1}ppb");
        }

    }
}