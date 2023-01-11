﻿using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Atmospheric;
using System;
using System.Threading.Tasks;

namespace Sensors.Atmospheric.Th02_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        Th02 sensor;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initializing...");

            sensor = new Th02(Device.CreateI2cBus());

            var consumer = Th02.CreateObserver(
                handler: result =>
                {
                    Resolver.Log.Info($"Observer: Temp changed by threshold; new temp: {result.New.Temperature?.Celsius:N2}C, old: {result.Old?.Temperature?.Celsius:N2}C");
                },
                filter: result =>
                {
                    //c# 8 pattern match syntax. checks for !null and assigns var.
                    if (result.Old is { } old)
                    {
                        return (
                        (result.New.Temperature.Value - old.Temperature.Value).Abs().Celsius > 0.5);
                    }
                    return false;
                }
            );
            sensor.Subscribe(consumer);

            sensor.Updated += (sender, result) =>
            {
                Resolver.Log.Info($"  Temperature: {result.New.Temperature?.Celsius:N2}C");
                Resolver.Log.Info($"  Relative Humidity: {result.New.Humidity?.Percent:N2}%");
            };

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            var conditions = await sensor.Read();
            Resolver.Log.Info($"Temperature: {conditions.Temperature?.Celsius}°C, Relative Humidity: {conditions.Humidity?.Percent}%");

            sensor.StartUpdating(TimeSpan.FromSeconds(1));
        }

        //<!=SNOP=>
    }
}