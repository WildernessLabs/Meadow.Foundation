using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.CAN;
using Meadow.Hardware;
using System;
using System.Threading.Tasks;

namespace Meadow.Devices
{
    public static class ProjLab
    {
        public static (
            IPin MB1_CS,
            IPin MB1_INT,
            IPin MB1_PWM,
            IPin MB1_AN,
            IPin MB1_SO,
            IPin MB1_SI,
            IPin MB1_SCK,
            IPin MB1_SCL,
            IPin MB1_SDA,

            IPin MB2_CS,
            IPin MB2_INT,
            IPin MB2_PWM,
            IPin MB2_AN,
            IPin MB2_SO,
            IPin MB2_SI,
            IPin MB2_SCK,
            IPin MB2_SCL,
            IPin MB2_SDA
            ) Pins = (
            Resolver.Device.GetPin("D14"),
            Resolver.Device.GetPin("D03"),
            Resolver.Device.GetPin("D04"),
            Resolver.Device.GetPin("A00"),
            Resolver.Device.GetPin("CIPO"),
            Resolver.Device.GetPin("COPI"),
            Resolver.Device.GetPin("SCK"),
            Resolver.Device.GetPin("D08"),
            Resolver.Device.GetPin("D07"),

            Resolver.Device.GetPin("A02"),
            Resolver.Device.GetPin("D04"),
            Resolver.Device.GetPin("D03"),
            Resolver.Device.GetPin("A01"),
            Resolver.Device.GetPin("CIPO"),
            Resolver.Device.GetPin("COPI"),
            Resolver.Device.GetPin("SCK"),
            Resolver.Device.GetPin("D08"),
            Resolver.Device.GetPin("D07")
            );
    }
}

namespace MeadowApp
{
    public class MeadowApp : App<F7FeatherV1>
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Main...");
            MeadowOS.Main(args);
        }

        //<!=SNIP=>

        Mcp2515 _controller;

        public override Task Initialize()
        {
            Console.WriteLine($"Init log level {Resolver.Log.Loglevel}");
            Resolver.Log.Info("Initialize...");

            //            var chipSelect = Device.CreateDigitalOutputPort(Device.Pins.MB1_CS(), true);
            var chipSelect = Device.CreateDigitalOutputPort(ProjLab.Pins.MB2_CS, true);

            Resolver.Log.Info($"CS = {chipSelect.Pin.Name}");

            _controller = new Mcp2515(Device.CreateSpiBus(), chipSelect);

            return base.Initialize();
        }

        public override Task Run()
        {
            return base.Run();
        }

        //<!=SNOP=>
    }
}