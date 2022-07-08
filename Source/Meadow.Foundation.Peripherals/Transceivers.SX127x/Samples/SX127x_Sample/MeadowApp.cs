using System;
using System.Threading;
using System.Threading.Tasks;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.Transceivers;

namespace MeadowApp
{
    public class MeadowApp : App<F7FeatherV2>
    {
        SX127x radio;

        public async override Task Initialize()
        {
            Console.WriteLine("Initialize...");

            var bus = Device.CreateSpiBus();
            var chipSelect = Device.CreateDigitalOutputPort(Device.Pins.D00);
            radio = new SX127x(bus, chipSelect);

             while (true)
            {
                // this is purely a test for supporting changing bus speed in SPI - remove when the radio is implemented
                foreach (var spd in bus.SupportedSpeeds)
                {
                    Console.WriteLine($"{spd}");

                    try
                    {
//                        bus.Configuration.SpeedKHz = spd;

                        Console.WriteLine($" @{bus.Configuration.Speed} Silicon Revision: 0x{radio.GetVersion():x2}");
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine($" {ex.Message}");
                    }

                    await Task.Delay(2000);
                }
            }
        }
    }
}