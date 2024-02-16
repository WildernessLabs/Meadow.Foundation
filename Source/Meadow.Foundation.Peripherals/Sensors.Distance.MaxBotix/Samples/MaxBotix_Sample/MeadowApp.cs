﻿using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Distance;
using Meadow.Units;
using System;
using System.Threading.Tasks;

namespace MaxBotix_Sample
{
    // Change F7FeatherV2 to F7FeatherV1 for V1.x boards
    public class MeadowApp : App<F7FeatherV2>
    {
        MaxBotix maxBotix;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            //Analog
            // maxBotix = new MaxBotix(Device, Device.Pins.A00, MaxBotix.SensorType.HR10Meter);

            //Serial
            //maxBotix = new MaxBotix(Device, Device.PlatformOS.GetSerialPortName("COM4"), MaxBotix.SensorType.XL);

            //I2C - don't forget external pull-up resistors 
            maxBotix = new MaxBotix(Device.CreateI2cBus(), MaxBotix.SensorType.HR10Meter);

            var consumer = MaxBotix.CreateObserver(
                handler: result =>
                {
                    Resolver.Log.Info($"Observer: Distance changed by threshold; new distance: {result.New.Centimeters:N2}cm, old: {result.Old?.Centimeters:N2}cm");
                },
                filter: result =>
                {
                    if (result.Old is { } old)
                    {
                        return Math.Abs((result.New - old).Centimeters) > 0.5;
                    }
                    return false;
                }
            );
            maxBotix.Subscribe(consumer);

            maxBotix.Updated += MaxBotix_DistanceUpdated;

            return Task.CompletedTask;
        }

        public override async Task Run()
        {
            var distance = await maxBotix.Read();
            Resolver.Log.Info($"Distance is: {distance.Centimeters}cm");

            maxBotix.StartUpdating(TimeSpan.FromSeconds(1));
        }

        private void MaxBotix_DistanceUpdated(object sender, IChangeResult<Length> e)
        {
            Resolver.Log.Info($"Length: {e.New.Centimeters}cm");
        }
    }
}