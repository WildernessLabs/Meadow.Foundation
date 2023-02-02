using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Environmental;

namespace Sensors.Environmental.Y4000_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Y4000 sensor;

        public async override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");
            await Task.Delay(2000);

            sensor = new Y4000(Device, Device.PlatformOS.GetSerialPortName("COM4"), 0x01, Device.Pins.D09);
            await sensor.Initialize();

            await Task.Delay(2000);
        }

        public override async Task Run()
        {
            Resolver.Log.Info("Run...");

            var isdn = await sensor.GetISDN();
            Resolver.Log.Info($"Address: {isdn}");

            var supplyVoltage = await sensor.GetSupplyVoltage();
            Resolver.Log.Info($"Supply voltage: {supplyVoltage}");

            var measurements = await sensor.Read();

            Resolver.Log.Info($"Sensor data: {measurements}");
        }

        //<!=SNOP=>
    }
}