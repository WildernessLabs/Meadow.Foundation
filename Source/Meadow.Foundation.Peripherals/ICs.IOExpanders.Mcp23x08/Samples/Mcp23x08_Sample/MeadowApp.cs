using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Hardware;
using Meadow.Foundation.ICs.IOExpanders;
using System.Collections.Generic;

namespace ICs.IOExpanders.Mcp23x08_Output_Sample
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {

        public MeadowApp()
        {
            var inte = Device.CreateDigitalInputPort(Device.Pins.D04, InterruptMode.EdgeRising);
            inte.Changed += (sender, args) => Console.WriteLine($"Interrupt changed to {args.Value}");

            Console.WriteLine("Ready.");
            //ReadInterrupt(inte);
        }

        private void ReadInterrupt(IDigitalInputPort interrupt)
        {
            while (true)
            {
                Console.WriteLine($"INTE {interrupt.State}");

                Thread.Sleep(1000);
            }
        }

    }
}
