﻿using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Relays;

namespace Relays.Relay_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        protected Relay relay;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            relay = new Relay(Device.CreateDigitalOutputPort(Device.Pins.D02));

            return Task.CompletedTask;
        }

        public override Task Run()
        {
            var state = false;

            while (true)
            {
                state = !state;

                Resolver.Log.Info($"- State: {state}");
                relay.IsOn = state;

                Thread.Sleep(500);
            }
        }

        //<!=SNOP=>
    }
}