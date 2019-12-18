﻿using Meadow;
using System.Threading;

namespace Sensors.Atmospheric.Mpl3115A2_Sample
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