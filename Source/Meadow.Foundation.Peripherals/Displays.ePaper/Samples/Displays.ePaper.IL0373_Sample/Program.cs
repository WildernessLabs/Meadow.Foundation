﻿using System.Threading;
using Meadow;

namespace Displays.ePaper.IL0373_Sample
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