using System;
using System.Threading;
using Meadow;

namespace Sensors.LoadCell.Hx711_Sample
{
    class Program
    {
        static IApp app;

        public static void Main(string[] args)
        {
            Console.WriteLine($"HX711 Load Cell Sample");

            app = new MeadowApp();

            Thread.Sleep(Timeout.Infinite);
        }
    }
}