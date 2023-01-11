﻿using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Temperature;
using System;
using System.Threading.Tasks;

namespace Sensors.Temperature.Lm75_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Lm75 lm75;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            lm75 = new Lm75(Device.CreateI2cBus());

            var consumer = Lm75.CreateObserver(
                handler: result =>
                {
                    Resolver.Log.Info($"Temperature New Value { result.New.Celsius}C");
                    Resolver.Log.Info($"Temperature Old Value { result.Old?.Celsius}C");
                      },
                filter: null
            );
            lm75.Subscribe(consumer);

            lm75.TemperatureUpdated += (object sender, IChangeResult<Meadow.Units.Temperature> e) =>
            {
                Resolver.Log.Info($"Temperature Updated: {e.New.Celsius:n2}C");
            };
            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            var temp = await lm75.Read();
            Resolver.Log.Info($"Temperature New Value {temp.Celsius}C");

            lm75.StartUpdating();
        }

        //<!=SNOP=>
    }
}