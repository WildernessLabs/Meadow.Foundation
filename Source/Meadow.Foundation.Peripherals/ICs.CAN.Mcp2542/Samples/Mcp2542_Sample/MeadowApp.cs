using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.CAN;
using System;
using System.Threading.Tasks;

namespace MeadowApp
{
    public class MeadowApp : App<F7FeatherV1>
    {
        public static async Task Main(string[] args)
        {
            Console.WriteLine("+Main");
            await MeadowOS.Main(args);
            Console.WriteLine("-Main");
        }

        //<!=SNIP=>

        Mcp2542 _can;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            Resolver.Log.Loglevel = Meadow.Logging.LogLevel.Trace;

            var port = Device.CreateSerialPort(Device.SerialPortNames.Com1, Mcp2542.DefaultBaudRate);
            var standby = Device.CreateDigitalOutputPort(ProjLab.Pins.MB1_AN);
            _can = new Mcp2542(port, standby, Resolver.Log);

            return base.Initialize();
        }

        public override async Task Run()
        {
            _can.Standby = false;

            while (true)
            {
                try
                {
                    var frame = _can.Read();

                    if (frame == null)
                    {
                        Resolver.Log.Info("No frames available");
                    }
                }
                catch (Exception ex)
                {
                    Resolver.Log.Error(ex.Message);
                }

                await Task.Delay(1000);

            }
        }

        //<!=SNOP=>
    }
}