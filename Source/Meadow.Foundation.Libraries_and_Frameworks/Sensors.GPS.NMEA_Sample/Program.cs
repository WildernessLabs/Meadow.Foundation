using Meadow;
using System.Threading;

namespace BasicSensors.GPS.NMEA_Sample
{
    class Program
    {
        static IApp app;
        public static void Main(string[] args)
        {
            // instantiate and run new meadow app
            app = new App();

            Thread.Sleep(Timeout.Infinite);
        }
    }
}