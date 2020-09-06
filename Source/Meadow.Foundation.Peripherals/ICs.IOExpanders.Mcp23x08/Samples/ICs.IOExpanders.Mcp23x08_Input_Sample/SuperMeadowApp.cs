using System;
using System.Collections.Generic;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Foundation.Leds;
using Meadow.Foundation.Sensors.Buttons;
using Meadow.Hardware;

namespace ICs.IOExpanders.Mcp23x08_Input_Sample
{
    public class SuperMeadowApp : App<F7Micro, SuperMeadowApp>
    {
        private readonly IMcp23x08 i2cMcp;
        private readonly Led[] leds;
        private readonly IMcp23x08 spiMcp1;
        private readonly IMcp23x08 spiMcp2;
        private long time;
        private readonly IDigitalInputPort[] buttons;

        public SuperMeadowApp()
        {
            Console.WriteLine("Initialize hardware...");

            var inte = Device.CreateDigitalInputPort(Device.Pins.D02, InterruptMode.EdgeBoth);
            McpLogger.LogToConsole();
            i2cMcp = new Mcp23x08(
                Device.CreateI2cBus(I2cBusSpeed.Fast),
                inte,
                false,
                false,
                false);

            buttons = new[]
            {
                i2cMcp.CreateDigitalInputPort(i2cMcp.Pins.GP0, InterruptMode.EdgeBoth, ResistorMode.PullUp),
                i2cMcp.CreateDigitalInputPort(i2cMcp.Pins.GP1, InterruptMode.EdgeBoth, ResistorMode.PullUp),
                i2cMcp.CreateDigitalInputPort(i2cMcp.Pins.GP2, InterruptMode.EdgeBoth, ResistorMode.PullUp),
                i2cMcp.CreateDigitalInputPort(i2cMcp.Pins.GP3, InterruptMode.EdgeBoth, ResistorMode.PullUp),
            };
            
            leds = new []
            {
                new Led(i2cMcp.CreateDigitalOutputPort(i2cMcp.Pins.GP4)),
                new Led(i2cMcp.CreateDigitalOutputPort(i2cMcp.Pins.GP5)),
                new Led(i2cMcp.CreateDigitalOutputPort(i2cMcp.Pins.GP6)),
                new Led(i2cMcp.CreateDigitalOutputPort(i2cMcp.Pins.GP7))
            };



            for (var i = 0; i < leds.Length; i++)
            {
                // scoping
                var current = i;

                var led = leds[current];
                var button = buttons[current];

                button.Changed += (sender, args) =>
                {
                    led.IsOn = !args.Value;
                };
            }

            //var spiBus = Device.CreateSpiBus(
            //    Device.Pins.SCK,
            //    Device.Pins.MOSI,
            //    Device.Pins.MISO,
            //    new SpiClockConfiguration(375, SpiClockConfiguration.Mode.Mode0));
            //Console.WriteLine($"SPI bus clock is {spiBus.Configuration.SpeedKHz} KHz");
            //spiMcp1 = new Mcp23x08(spiBus, Device.CreateDigitalOutputPort(Device.Pins.D00, true), false, false);
            //spiMcp2 = new Mcp23x08(spiBus, Device.CreateDigitalOutputPort(Device.Pins.D01, true), true, false);

            //leds = new List<Led>();
            //leds.Add(new Led(i2cMcp.CreateDigitalOutputPort(i2cMcp.Pins.GP4)));
            //leds.Add(new Led(i2cMcp.CreateDigitalOutputPort(i2cMcp.Pins.GP5)));
            //leds.Add(new Led(i2cMcp.CreateDigitalOutputPort(i2cMcp.Pins.GP6)));
            //leds.Add(new Led(i2cMcp.CreateDigitalOutputPort(i2cMcp.Pins.GP7)));

            //leds.Add(new Led(spiMcp1.CreateDigitalOutputPort(spiMcp1.Pins.GP0)));
            //leds.Add(new Led(spiMcp1.CreateDigitalOutputPort(spiMcp1.Pins.GP1)));
            //leds.Add(new Led(spiMcp1.CreateDigitalOutputPort(spiMcp1.Pins.GP2)));
            //leds.Add(new Led(spiMcp1.CreateDigitalOutputPort(spiMcp1.Pins.GP3)));
            //leds.Add(new Led(spiMcp1.CreateDigitalOutputPort(spiMcp1.Pins.GP4)));
            //leds.Add(new Led(spiMcp1.CreateDigitalOutputPort(spiMcp1.Pins.GP5)));
            //leds.Add(new Led(spiMcp1.CreateDigitalOutputPort(spiMcp1.Pins.GP6)));
            //leds.Add(new Led(spiMcp1.CreateDigitalOutputPort(spiMcp1.Pins.GP7)));

            //leds.Add(new Led(spiMcp2.CreateDigitalOutputPort(spiMcp2.Pins.GP0)));
            //leds.Add(new Led(spiMcp2.CreateDigitalOutputPort(spiMcp2.Pins.GP1)));
            //leds.Add(new Led(spiMcp2.CreateDigitalOutputPort(spiMcp2.Pins.GP2)));
            //leds.Add(new Led(spiMcp2.CreateDigitalOutputPort(spiMcp2.Pins.GP3)));
            //leds.Add(new Led(spiMcp2.CreateDigitalOutputPort(spiMcp2.Pins.GP4)));
            //leds.Add(new Led(spiMcp2.CreateDigitalOutputPort(spiMcp2.Pins.GP5)));
            //leds.Add(new Led(spiMcp2.CreateDigitalOutputPort(spiMcp2.Pins.GP6)));
            //leds.Add(new Led(spiMcp2.CreateDigitalOutputPort(spiMcp2.Pins.GP7)));


            Console.WriteLine("done.");

            //CycleLeds();
            //ReadButtons();

        }

        private void ReadButtons()
        {
            while (true)
            {
                foreach (var button in buttons)
                {
                    Console.Write(button.State ? 0 : 1);
                }

                Console.Write("\n");

                Thread.Sleep(1000);
            }
        }

        private void CycleLeds()
        {
            Console.WriteLine("Cycle leds...");

            while (true)
            {
                foreach (var led in leds)
                {
                    led.IsOn = true;
                    WaitTime();
                }

                foreach (var led in leds)
                {
                    led.IsOn = false;
                    WaitTime();
                }
            }
        }

        private void WaitTime()
        {
            var x = (int) (Math.Cos(time / 5000.0) * 100 + 101);

            time += x;
            Thread.Sleep(x);
        }
    }
}
