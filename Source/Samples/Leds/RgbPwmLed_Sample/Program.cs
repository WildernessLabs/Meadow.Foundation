using Meadow;
using System.Threading;

namespace RgbPwmLed_Sample
{
    class Program
    {
        static IApp app;

        static void Main(string[] args)
        {
            app = new RgbPwmLedApp();
            Thread.Sleep(Timeout.Infinite);
        }
    }
}