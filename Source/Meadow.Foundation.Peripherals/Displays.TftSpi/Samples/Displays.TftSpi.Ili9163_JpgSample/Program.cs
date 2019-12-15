using System.Threading;
using Meadow;

namespace Displays.TftSpi.Ili9341_JpgSample
{
    class Program
    {
        static IApp app;
        public static void Main(string[] args)
        {
            // instantiate and run new meadow app
            app = new MeadowApp();

            Thread.Sleep(Timeout.Infinite);
        }
    }
}
