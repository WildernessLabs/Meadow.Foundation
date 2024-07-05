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
        public static async Task Main(string[] args)
        {
            Console.WriteLine("+Main");
            await MeadowOS.Main(args);
            Console.WriteLine("-Main");
        }

        //<!=SNIP=>

        Mcp2515 _controller;

        public override Task Initialize()
        {
            Resolver.Log.Info("Initialize...");

            Resolver.Log.Loglevel = Meadow.Logging.LogLevel.Trace;

            //            var chipSelect = Device.CreateDigitalOutputPort(Device.Pins.MB1_CS(), true);
            var chipSelect = Device.CreateDigitalOutputPort(ProjLab.Pins.MB2_CS, true);

            Resolver.Log.Info($"CS = {chipSelect.Pin.Name}");
            var spi = Device.CreateSpiBus();
            spi.Configuration.Phase = SpiClockConfiguration.ClockPhase.Zero;
            spi.Configuration.Polarity = SpiClockConfiguration.ClockPolarity.Normal;
            spi.Configuration.Speed = new Meadow.Units.Frequency(250, Meadow.Units.Frequency.UnitType.Kilohertz);

            _controller = new Mcp2515(spi, chipSelect, Resolver.Log);

            return base.Initialize();
        }

        public override async Task Run()
        {
            while (true)
            {
                try
                {
                    var frame = _controller.ReadFrame();

                    if (frame == null)
                    {
                        Resolver.Log.Info("No frames available");
                    }
                    else
                    {
                        Resolver.Log.Info($"ID: {frame.Value.ID}");
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