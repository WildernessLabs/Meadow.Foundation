using System;
using System.Threading;
using Meadow;
using Meadow.Devices;
using Meadow.Hardware;

namespace MeadowApp
{
    public class MeadowApp : App<F7Micro, MeadowApp>
    {
        public MeadowApp()
        {
            ConfigurePorts();
        }

        public void ConfigurePorts()
        {
            Console.WriteLine("Creating output ports...");
        }
    }
}