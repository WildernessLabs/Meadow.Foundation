﻿using System;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Weather;
using Meadow.Units;

namespace MeadowApp
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        SwitchingRainGauge rainGauge;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            // initialize the rain gauge driver
            rainGauge = new SwitchingRainGauge(Device, Device.Pins.D15);

            //==== Classic event example:
            rainGauge.Updated += (sender, result) => Resolver.Log.Info($"Updated event {result.New.Millimeters}mm");

            //==== IObservable Pattern
            var observer = SwitchingRainGauge.CreateObserver(
                handler: result => Resolver.Log.Info($"Rain depth: {result.New.Millimeters}mm"),
                filter: null
            );
            rainGauge.Subscribe(observer);

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            // get initial reading, just to test the API - should be 0
            Length rainFall = await rainGauge.Read();
            Resolver.Log.Info($"Initial depth: {rainFall.Millimeters}mm");

            // start the sensor
            rainGauge.StartUpdating();
        }

        //<!=SNOP=>
    }
}