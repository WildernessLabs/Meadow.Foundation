using Meadow;
using System.Threading;

namespace RgbLed_Sample
{
    class Program
    {
        static IApp app;

        static void Main(string[] args)
        {
            app = new RgbLedApp();
            Thread.Sleep(Timeout.Infinite);
        }
    }
}