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

        Ina219 ina219;

        public override Task Initialize()
        {
            Resolver.Log.Debug("Initialize...");

            var bus = Device.CreateI2cBus(I2cBusSpeed.Fast);
            ina219 = new Ina219(bus);
            ina219.Configure(busVoltageRange: Ina219.BusVoltageRange.Range_32V,
                maxExpectedCurrent: new Current(1.0),
                adcMode: Ina219.ADCModes.ADCMode_4xAvg_2128us);
            Resolver.SensorService.RegisterSensor(ina219);

            Resolver.Log.Info($"--- INA219 Sample App ---");
            ina219.Updated += (sender, result) =>
            {
                Resolver.Log.Info($"{result.New.Voltage:N3} V @ {result.New.Current:N3} A");
            };

            return Task.CompletedTask;
        }

        public override Task Run()
        {
            Resolver.Log.Debug("Run...");
            ina219.StartUpdating(TimeSpan.FromSeconds(2));
            return Task.CompletedTask;
        }

        //<!=SNOP=>
    }
}