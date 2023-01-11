﻿using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Temperature;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace Sensors.Temperature.Thermistor_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        private SteinhartHartCalculatedThermistor thermistor;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            thermistor = new SteinhartHartCalculatedThermistor(Device.CreateAnalogInputPort(Device.Pins.A00), new Resistance(10, Meadow.Units.Resistance.UnitType.Kiloohms));

            var consumer = SteinhartHartCalculatedThermistor.CreateObserver(
                handler: result =>
                {
                    Resolver.Log.Info($"Temperature New Value {result.New.Fahrenheit:N1}F/{result.New.Celsius:N1}C");
                    Resolver.Log.Info($"Temperature Old Value {result.Old?.Fahrenheit:N1}F/{result.Old?.Celsius:N1}C");
                },
                filter: null
            );
            thermistor.Subscribe(consumer);

            thermistor.TemperatureUpdated += (object sender, IChangeResult<Meadow.Units.Temperature> e) =>
            {
                Resolver.Log.Info($"Temperature Updated: {e.New.Fahrenheit:N1}F/{e.New.Celsius:N1}C");
            };

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            var temp = await thermistor.Read();
            Resolver.Log.Info($"Current temperature: {temp.Fahrenheit:N1}F/{temp.Celsius:N1}C");

            thermistor.StartUpdating(TimeSpan.FromSeconds(1));
        }

        //<!=SNOP=>
    }
}