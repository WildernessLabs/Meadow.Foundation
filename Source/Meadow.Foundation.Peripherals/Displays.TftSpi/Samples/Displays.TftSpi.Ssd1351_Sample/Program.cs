using System.Threading;
using Meadow;

namespace BasicDisplays.Tft.Ssd1351_Sample
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