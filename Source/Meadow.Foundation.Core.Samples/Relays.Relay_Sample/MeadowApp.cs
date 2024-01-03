using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Relays;
using Meadow.Peripherals.Relays;
using System.Threading;
using System.Threading.Tasks;

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
            while (true)
            {
                var newState = relay.State switch
                {
                    RelayState.Open => RelayState.Closed,
                    _ => RelayState.Open
                };

                Resolver.Log.Info($"- State: {newState}");
                relay.State = newState;

                Thread.Sleep(500);
            }
        }

        //<!=SNOP=>
    }
}