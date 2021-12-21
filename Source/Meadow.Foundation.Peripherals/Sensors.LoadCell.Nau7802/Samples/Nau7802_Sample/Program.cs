using System;
using System.Threading;
using Meadow;

namespace Sensors.LoadCell.Nau7802_Sample
{
    class Program
    {
        static IApp app;

        public static void Main(string[] args)
        {
            Console.WriteLine($"NAU7802 Load Cell Sample");

            app = new MeadowApp();

            Thread.Sleep(Timeout.Infinite);
        }
    }
}