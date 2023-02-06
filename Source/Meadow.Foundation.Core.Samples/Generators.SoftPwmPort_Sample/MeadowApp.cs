using Meadow;
using Meadow.Devices;
using Meadow.Hardware;
using System.Threading;
using System.Threading.Tasks;

namespace Generators.SoftPwmPort_Sample
{
    public class MeadowApp : App<F7FeatherV2>
    {
        //<!=SNIP=>

        protected SoftPwmPort softPwmPort;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initializing...");

            IDigitalOutputPort digiOut = Device.Pins.D00.CreateDigitalOutputPort();
            softPwmPort = new SoftPwmPort(digiOut);

            return Task.CompletedTask;
        }

        public override Task Run()
        {
            TestSoftPwmPort();
            return Task.CompletedTask;
        }

        protected void TestSoftPwmPort()
        {
            Resolver.Log.Info("TestSoftPwmPort...");

            softPwmPort.Start();

            while (true)
            {
                softPwmPort.DutyCycle = 0.2f;
                Thread.Sleep(500);
                softPwmPort.DutyCycle = 0.5f;
                Thread.Sleep(500);
                softPwmPort.DutyCycle = 0.75f;
                Thread.Sleep(500);
                softPwmPort.DutyCycle = 1f;
                Thread.Sleep(500);
            }
        }

        //<!=SNOP=>
    }
}
