using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Power;
using System;
using System.Threading.Tasks;
using Meadow.Hardware;

namespace MeadowApp
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Ina260 ina260;

        public override Task Initialize()
        {
            Resolver.Log.Debug("Initialize...");

            var bus = Device.CreateI2cBus(I2cBusSpeed.Fast);
            ina260 = new Ina260(bus);
            ina260.Configure(Ina260.ConversionTime.ConversionTime_8244us, Ina260.ConversionTime.ConversionTime_8244us, Ina260.Averaging.Average_4);
            //Resolver.SensorService.RegisterSensor(ina260);

            Resolver.Log.Info($"--- INA260 Sample App ---");
            Resolver.Log.Info($"Manufacturer: {ina260.ManufacturerID}");
            Resolver.Log.Info($"DeviceID: 0x{ina260.DeviceID:X3}");
            Resolver.Log.Info($"Revision: 0x{ina260.DeviceRevision:X2}");
            ina260.Updated += (sender, result) =>
            {
                Resolver.Log.Info($"{result.New.Voltage:N3} V @ {result.New.Current:N3} A");
            };

            return Task.CompletedTask;
        }

        public override Task Run()
        {
            Resolver.Log.Debug("Run...");
            ina260.StartUpdating(TimeSpan.FromSeconds(2));
            return Task.CompletedTask;
        }

        //<!=SNOP=>
    }
}