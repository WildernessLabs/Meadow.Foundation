using System;
using System.Threading;
using Meadow;

namespace PwmLed_Sample
{
    class MainClass
    {
        static IApp app;

        public static void Main(string[] args)
        {
            app = new PwmLedApp();
            Thread.Sleep(Timeout.Infinite);
        }
    }
}
