﻿using System.Threading;
using Meadow;

namespace Leds.PwmLed_Onboard_Sample
{
    class Program
    {
        static IApp app;

        public static void Main(string[] args)
        {
            app = new MeadowApp();

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
