using System;
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
            Console.WriteLine("Initialize hardware...");

            relay = new Relay(Device.CreateDigitalOutputPort(Device.Pins.D02));

            return Task.CompletedTask;
        }

        public override Task Run()
        {
            var state = false;

            while (true)
            {
                state = !state;

                Console.WriteLine($"- State: {state}");
                relay.IsOn = state;

                Thread.Sleep(500);
            }
        }

        //<!=SNOP=>
    }
}