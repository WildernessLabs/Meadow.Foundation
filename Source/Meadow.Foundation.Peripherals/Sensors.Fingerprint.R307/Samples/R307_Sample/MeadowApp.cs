using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Sensors.Fingerprint;
using System;
using System.Threading.Tasks;

namespace Sensors.Fingerprint.R307_Sample
{
    public class MeadowApp : App<F7CoreComputeV2>
    {
        //<!=SNIP=>

        private R307 sensor;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initializing ...");

            var port = Device.PlatformOS
                .GetSerialPortName("com1")
                .CreateSerialPort(R307.DefaultBaudRate);

            sensor = new R307(port, Device.Pins.PC2);

            var connected = false;

            do
            {
                try
                {
                    sensor.Enable();
                    connected = true;
                }
                catch (TimeoutException)
                {
                    Resolver.Log.Info("Timeout.  Check your wiring.");
                }
            } while (!connected);

            return Task.CompletedTask;
        }

        public override Task Run()
        {

            return Task.CompletedTask;
        }

        //<!=SNOP=>
    }
}