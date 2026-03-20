using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Motion;
using System;
using System.Threading.Tasks;

namespace Sensors.Motion.Ak9753_Sample
{
    public class MeadowApp : App<F7CoreComputeV2>
    {
        //<!=SNIP=>

        private Ak9753 sensor;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            sensor = new Ak9753(Device.CreateI2cBus(3));

            sensor.Updated += OnSensorUpdated;

            return Task.CompletedTask;
        }

        public override Task Run()
        {
            sensor.StartUpdating(TimeSpan.FromSeconds(1));

            return Task.CompletedTask;
        }

        private void OnSensorUpdated(object sender, IChangeResult<Ak9753SensorData> result)
        {
            var d = result.New;
            Resolver.Log.Info($"IR1 (Down):{d.Ir1,6}  IR2 (Left):{d.Ir2,6}  IR3 (Up):{d.Ir3,6}  IR4 (Right):{d.Ir4,6}  Temp:{d.Temperature.Celsius:F2}°C");
        }

        //<!=SNOP=>
    }
}
