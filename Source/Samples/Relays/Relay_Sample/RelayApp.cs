using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Relays;
using System;
using System.Threading;

namespace Relay_Sample
{
    public class RelayApp : App<F7Micro, RelayApp>
    {
        Relay relay;

        public RelayApp()
        {
            relay = new Relay(Device.CreateDigitalOutputPort(Device.Pins.D02));

            TestRelay();
        }

        protected void TestRelay()
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
    }
}