using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.IOExpanders;
using System.Threading;
using System.Threading.Tasks;

namespace ICs.IOExpanders.Ds3502_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        protected Ds3502 ds3502;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            ds3502 = new Ds3502(Device.CreateI2cBus(Ds3502.DefaultBusSpeed));

            return base.Initialize();
        }

        public override Task Run()
        {
            for (byte i = 0; i < 127; i++)
            {
                ds3502.SetWiper(i);
                Resolver.Log.Info($"wiper {ds3502.GetWiper()}");

                Thread.Sleep(1000);
            }

            return base.Run();
        }

        //<!=SNOP=>
    }
}