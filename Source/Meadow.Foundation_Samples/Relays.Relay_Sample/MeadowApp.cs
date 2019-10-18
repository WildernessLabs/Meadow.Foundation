using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Relays;

namespace Relays.Relay_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        protected Relay relay;

        public MeadowApp()
        {
            Console.WriteLine("Initializing...");

            relay = new Relay(Device.CreateDigitalOutputPort(Device.Pins.D02));

            TestRelay();
        }

        protected void TestRelay()
        {
            Console.WriteLine("TestRelay...");

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