using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Temperature;
using System.Threading.Tasks;

namespace Sensors.Temperature.MCP9601_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Mcp9601 sensor;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            sensor = new Mcp9601(Device.CreateI2cBus());

            var consumer = Mcp9601.CreateObserver(
                handler: result =>
                {
                    Resolver.Log.Info($"Temperature New Value {result.New.TemperatureHot.Value.Celsius}C");
                },
                filter: null
            );
            sensor.Subscribe(consumer);

            sensor.Updated += Sensor_Updated;
            return Task.CompletedTask;
        }

        private void Sensor_Updated(object sender, IChangeResult<(Meadow.Units.Temperature? TemperatureHot, Meadow.Units.Temperature? TemperatureCold)> e)
        {
            Resolver.Log.Info($"Temperature hot: {e.New.TemperatureHot.Value.Celsius:n2}C, Temperature cold: {e.New.TemperatureCold.Value.Celsius:n2}C");
        }

        public override Task Run()
        {
            sensor.StartUpdating();
            return Task.CompletedTask;
        }

        //<!=SNOP=>
    }
}