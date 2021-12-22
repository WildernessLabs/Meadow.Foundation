using Meadow;
using System;
using System.Threading;

namespace Ads1015_Sample
{
    internal class Program
    {
        static IApp app;
        public static void Main(string[] args)
        {
            Console.WriteLine("Meadow ADS1115 Sample Application");

            app = new MeadowApp();

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
