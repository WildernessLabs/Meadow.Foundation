using Meadow;
using System.Threading;

namespace Sensors.Temperature.AnalogTemperature_Sample
{
    class Program
    {
        static IApp app;
        public static void Main(string[] args)
        {
            if (args.Length > 0 && args[0] == "--exitOnDebug") return;

            // instantiate and run new meadow app
            app = new MeadowApp();

            // keep app alive
            Thread.Sleep(Timeout.Infinite);
        }
    }
}
