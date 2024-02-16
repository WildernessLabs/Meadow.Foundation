﻿using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Atmospheric;
using System;
using System.Threading.Tasks;

namespace Bh1900Nux_Sample
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        private Bh1900Nux _sensor;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initializing...");

            _sensor = new Bh1900Nux(Device.CreateI2cBus(), Bh1900Nux.Addresses.Default);

            var consumer = Bh1900Nux.CreateObserver(
                handler: result =>
                {
                    Resolver.Log.Info($"Observer: Temp changed by threshold; new temp: {result.New.Celsius:N2}C, old: {result.Old?.Celsius:N2}C");
                },
                filter: result =>
                {
                    if (result.Old is { } old)
                    {
                        return (result.New - old).Abs().Celsius > 0.5;
                    }
                    return false;
                }
            );
            _sensor.Subscribe(consumer);

            _sensor.Updated += (sender, result) =>
            {
                Resolver.Log.Info($"  Temperature: {result.New.Celsius:N2}C");
            };

            return Task.CompletedTask;
        }

        public async override Task Run()
        {
            var conditions = await _sensor.Read();
            Resolver.Log.Info("Initial Readings:");
            Resolver.Log.Info($"  Temperature: {conditions.Celsius:N2}C");

            _sensor.StartUpdating(TimeSpan.FromSeconds(1));
        }

        //<!=SNOP=>
    }
}