using System;
using System.Collections.Generic;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Foundation.Leds;

namespace ICs.IOExpanders.Mcp23x08_Input_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        private readonly List<Led> leds;
        private readonly IMcp23x08 mcp;
        private long time;

        public MeadowApp()
        {
            Console.Write("Initialize hardware...");

            mcp = new Mcp23x08(Device.CreateI2cBus(), false, false, false);

            leds = new List<Led>();
            //leds.Add(new Led(Device.CreateDigitalOutputPort(Device.Pins.D04)));
            //leds.Add(new Led(Device.CreateDigitalOutputPort(Device.Pins.D03)));
            //leds.Add(new Led(mcp.CreateDigitalOutputPort(mcp.Pins.GP7)));
            //leds.Add(new Led(mcp.CreateDigitalOutputPort(mcp.Pins.GP6)));
            //leds.Add(new Led(mcp.CreateDigitalOutputPort(mcp.Pins.GP5)));
            //leds.Add(new Led(mcp.CreateDigitalOutputPort(mcp.Pins.GP4)));
            //leds.Add(new Led(mcp.CreateDigitalOutputPort(mcp.Pins.GP3)));
            //leds.Add(new Led(mcp.CreateDigitalOutputPort(mcp.Pins.GP2)));
            //leds.Add(new Led(mcp.CreateDigitalOutputPort(mcp.Pins.GP1)));
            //leds.Add(new Led(mcp.CreateDigitalOutputPort(mcp.Pins.GP0)));
            throw new NotImplementedException();
            Console.WriteLine("done.");
            CycleLeds();
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
