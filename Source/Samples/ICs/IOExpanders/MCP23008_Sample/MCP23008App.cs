using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Foundation.ICs.IOExpanders;
using Meadow.Hardware;

namespace MCP23008_Sample
{
    public class MCP23008App : App<F7Micro, MCP23008App>
    {
        IDigitalOutputPort redLed;
        IDigitalOutputPort blueLed;
        IDigitalOutputPort greenLed;

        MCP23008 mcp23008;

        public MCP23008App()
        {
            mcp23008 = new MCP23008(Device.CreateI2cBus());

            BlinkLeds();
        }

        public void BlinkLeds()
        {
            // create an array of ports
            IDigitalOutputPort[] ports = new IDigitalOutputPort[8];
            for (byte i = 0; i <= 7; i++)
            {
                ports[i] = mcp23008.CreateOutputPort(i, false);
            }
            while (true)
            {
                // count from 0 to 7 (8 leds)
                for (int i = 0; i <= 7; i++)
                {
                    // turn on the LED that matches the count
                    for (byte j = 0; j <= 7; j++)
                    {
                        ports[j].State = (i == j);
                    }
                    Console.WriteLine("i: " + i.ToString());
                    Thread.Sleep(250);
                }
            }
        }
    }
}
