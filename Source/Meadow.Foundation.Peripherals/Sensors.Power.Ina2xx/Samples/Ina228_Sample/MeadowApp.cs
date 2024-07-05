using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Power;
using System;
using System.Threading.Tasks;
using Meadow.Hardware;
using Meadow.Units;

namespace MeadowApp
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Ina228 ina228;

        public override Task Initialize()
        {
            Resolver.Log.Debug("Initialize...");

            var bus = Device.CreateI2cBus(I2cBusSpeed.Fast);
            ina228 = new Ina228(bus);
            ina228.ConfigureConversion(averaging:Ina228.Averaging.Average_4);
            ina228.SetCalibration(new Current(10.0, Current.UnitType.Amps), false);
            Resolver.SensorService.RegisterSensor(ina228);

            Resolver.Log.Info($"--- INA228 Sample App ---");
            Resolver.Log.Info($"Manufacturer: {ina228.ManufacturerID}");
            Resolver.Log.Info($"DeviceID: 0x{ina228.DeviceID:X3}");
            Resolver.Log.Info($"Revision: 0x{ina228.DeviceRevision:X2}");
            ina228.Updated += (sender, result) =>
            {
                if (result.New is { Current: { }, Voltage: { } })
                    Resolver.Log.Info($"{result.New.Voltage.Value.Millivolts:N3} mV @ {result.New.Current.Value.Milliamps:N3} mA");
            };

            return Task.CompletedTask;
        }

        public override Task Run()
        {
            Resolver.Log.Debug("Run..."); 
            ina228.StartUpdating(TimeSpan.FromSeconds(2));
            return Task.CompletedTask;
        }

        //<!=SNOP=>
    }
}