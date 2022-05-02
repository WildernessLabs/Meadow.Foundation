using Meadow;
using System.Threading;

namespace BasicSensors.Atmospheric.Sht4x_Sample
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