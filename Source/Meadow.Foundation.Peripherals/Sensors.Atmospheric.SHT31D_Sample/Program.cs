using Meadow;
using System.Threading;

namespace BasicSensors.Atmospheric.SHT31D_Sample
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